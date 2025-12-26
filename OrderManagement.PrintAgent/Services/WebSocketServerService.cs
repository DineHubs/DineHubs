using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.PrintAgent.Models;

namespace OrderManagement.PrintAgent.Services;

/// <summary>
/// WebSocket server that listens for print requests from the Angular frontend
/// </summary>
public class WebSocketServerService : BackgroundService
{
    private readonly ILogger<WebSocketServerService> _logger;
    private readonly PrinterManager _printerManager;
    private readonly PrintAgentOptions _options;
    private readonly HttpListener _httpListener;
    private readonly List<WebSocket> _connectedClients = [];
    private readonly object _clientsLock = new();

    public WebSocketServerService(
        ILogger<WebSocketServerService> logger,
        PrinterManager printerManager,
        IOptions<PrintAgentOptions> options)
    {
        _logger = logger;
        _printerManager = printerManager;
        _options = options.Value;
        _httpListener = new HttpListener();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var prefix = $"http://localhost:{_options.WebSocketPort}/";
        _httpListener.Prefixes.Add(prefix);

        try
        {
            _httpListener.Start();
            _logger.LogInformation("Print Agent WebSocket server started on {Prefix}", prefix);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleWebSocketConnectionAsync(context, stoppingToken);
                    }
                    else
                    {
                        // Handle regular HTTP requests (for health checks, etc.)
                        await HandleHttpRequestAsync(context);
                    }
                }
                catch (HttpListenerException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting connection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start WebSocket server");
        }
        finally
        {
            _httpListener.Stop();
            _httpListener.Close();
        }
    }

    private async Task HandleWebSocketConnectionAsync(HttpListenerContext context, CancellationToken stoppingToken)
    {
        WebSocket? webSocket = null;

        try
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            webSocket = wsContext.WebSocket;

            lock (_clientsLock)
            {
                _connectedClients.Add(webSocket);
            }

            _logger.LogInformation("WebSocket client connected. Total clients: {Count}", _connectedClients.Count);

            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessageAsync(webSocket, message, stoppingToken);
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "WebSocket error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket connection");
        }
        finally
        {
            if (webSocket != null)
            {
                lock (_clientsLock)
                {
                    _connectedClients.Remove(webSocket);
                }

                if (webSocket.State != WebSocketState.Closed)
                {
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                    catch { }
                }

                webSocket.Dispose();
                _logger.LogInformation("WebSocket client disconnected. Total clients: {Count}", _connectedClients.Count);
            }
        }
    }

    private async Task ProcessMessageAsync(WebSocket webSocket, string message, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogDebug("Received message: {Message}", message);

            var printJob = JsonSerializer.Deserialize<PrintJob>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (printJob == null)
            {
                await SendResponseAsync(webSocket, new PrintResult
                {
                    Success = false,
                    Message = "Invalid print job format"
                }, stoppingToken);
                return;
            }

            PrintResult result;

            switch (printJob.Type?.ToLowerInvariant())
            {
                case "receipt":
                    if (printJob.Data == null)
                    {
                        result = new PrintResult { Success = false, Message = "Missing print data" };
                    }
                    else
                    {
                        result = await _printerManager.PrintReceiptAsync(printJob.Data, printJob.PrinterName);
                    }
                    break;

                case "kitchen":
                    if (printJob.Data == null)
                    {
                        result = new PrintResult { Success = false, Message = "Missing print data" };
                    }
                    else
                    {
                        result = await _printerManager.PrintKitchenTicketAsync(printJob.Data, printJob.PrinterName);
                    }
                    break;

                case "test":
                    result = await _printerManager.PrintTestPageAsync(printJob.PrinterName);
                    break;

                case "drawer":
                    result = await _printerManager.OpenCashDrawerAsync(printJob.PrinterName);
                    break;

                default:
                    result = new PrintResult { Success = false, Message = $"Unknown print job type: {printJob.Type}" };
                    break;
            }

            await SendResponseAsync(webSocket, result, stoppingToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing print job");
            await SendResponseAsync(webSocket, new PrintResult
            {
                Success = false,
                Message = "Invalid JSON format"
            }, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing print job");
            await SendResponseAsync(webSocket, new PrintResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            }, stoppingToken);
        }
    }

    private static async Task SendResponseAsync(WebSocket webSocket, PrintResult result, CancellationToken stoppingToken)
    {
        var json = JsonSerializer.Serialize(result);
        var bytes = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
    }

    private async Task HandleHttpRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;

        try
        {
            // Health check endpoint
            if (context.Request.Url?.AbsolutePath == "/health")
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";
                var body = Encoding.UTF8.GetBytes("{\"status\":\"healthy\",\"connectedClients\":" + _connectedClients.Count + "}");
                await response.OutputStream.WriteAsync(body);
            }
            // Printers list endpoint
            else if (context.Request.Url?.AbsolutePath == "/printers")
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";
                var printers = _printerManager.GetInstalledPrinters();
                var json = JsonSerializer.Serialize(printers);
                var body = Encoding.UTF8.GetBytes(json);
                await response.OutputStream.WriteAsync(body);
            }
            else
            {
                response.StatusCode = 404;
                var body = Encoding.UTF8.GetBytes("{\"error\":\"Not found\"}");
                await response.OutputStream.WriteAsync(body);
            }
        }
        finally
        {
            response.Close();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Print Agent WebSocket server...");

        // Close all connected clients
        lock (_clientsLock)
        {
            foreach (var client in _connectedClients)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait(1000);
                    }
                    client.Dispose();
                }
                catch { }
            }
            _connectedClients.Clear();
        }

        await base.StopAsync(cancellationToken);
    }
}

