using System.Drawing.Printing;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.PrintAgent.Models;

namespace OrderManagement.PrintAgent.Services;

/// <summary>
/// Manages printer connections and sends raw print data
/// </summary>
public class PrinterManager
{
    private readonly ILogger<PrinterManager> _logger;
    private readonly EscPosGenerator _escPosGenerator;
    private readonly PrintAgentOptions _options;

    public PrinterManager(
        ILogger<PrinterManager> logger,
        EscPosGenerator escPosGenerator,
        IOptions<PrintAgentOptions> options)
    {
        _logger = logger;
        _escPosGenerator = escPosGenerator;
        _options = options.Value;
    }

    public async Task<PrintResult> PrintReceiptAsync(PrintJobData data, string? printerName = null)
    {
        try
        {
            var printData = _escPosGenerator.GenerateReceipt(data);
            return await SendToPrinterAsync(printData, printerName, "receipt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing receipt for order {OrderNumber}", data.OrderNumber);
            return new PrintResult
            {
                Success = false,
                Message = $"Failed to print receipt: {ex.Message}"
            };
        }
    }

    public async Task<PrintResult> PrintKitchenTicketAsync(PrintJobData data, string? printerName = null)
    {
        try
        {
            var printData = _escPosGenerator.GenerateKitchenTicket(data);
            return await SendToPrinterAsync(printData, printerName, "kitchen");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing kitchen ticket for order {OrderNumber}", data.OrderNumber);
            return new PrintResult
            {
                Success = false,
                Message = $"Failed to print kitchen ticket: {ex.Message}"
            };
        }
    }

    public async Task<PrintResult> PrintTestPageAsync(string? printerName = null)
    {
        try
        {
            var printData = _escPosGenerator.GenerateTestPage();
            return await SendToPrinterAsync(printData, printerName, "test");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing test page");
            return new PrintResult
            {
                Success = false,
                Message = $"Failed to print test page: {ex.Message}"
            };
        }
    }

    public async Task<PrintResult> OpenCashDrawerAsync(string? printerName = null)
    {
        try
        {
            var command = _escPosGenerator.GetOpenDrawerCommand();
            return await SendToPrinterAsync(command, printerName, "drawer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening cash drawer");
            return new PrintResult
            {
                Success = false,
                Message = $"Failed to open cash drawer: {ex.Message}"
            };
        }
    }

    private async Task<PrintResult> SendToPrinterAsync(byte[] data, string? printerName, string jobType)
    {
        var printJobId = Guid.NewGuid().ToString();

        if (string.IsNullOrEmpty(printerName))
        {
            _logger.LogWarning("No printer specified, using default printer");
            // Try to get default printer
            printerName = GetDefaultPrinter();
        }

        if (string.IsNullOrEmpty(printerName))
        {
            return new PrintResult
            {
                Success = false,
                Message = "No printer configured",
                PrintJobId = printJobId
            };
        }

        // Check if it's a network printer (IP:Port format)
        if (printerName.Contains(':'))
        {
            return await SendToNetworkPrinterAsync(data, printerName, printJobId);
        }

        // Otherwise, try Windows printer
        return await SendToWindowsPrinterAsync(data, printerName, printJobId, jobType);
    }

    private async Task<PrintResult> SendToNetworkPrinterAsync(byte[] data, string address, string printJobId)
    {
        try
        {
            var parts = address.Split(':');
            var ip = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 9100;

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, port);

            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
            {
                return new PrintResult
                {
                    Success = false,
                    Message = $"Connection timeout to printer at {address}",
                    PrintJobId = printJobId
                };
            }

            await connectTask; // Ensure any exceptions are thrown

            using var stream = client.GetStream();
            await stream.WriteAsync(data);
            await stream.FlushAsync();

            _logger.LogInformation("Print job {PrintJobId} sent to network printer at {Address}", printJobId, address);

            return new PrintResult
            {
                Success = true,
                Message = "Print job sent successfully",
                PrintJobId = printJobId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending to network printer at {Address}", address);
            return new PrintResult
            {
                Success = false,
                Message = $"Failed to send to printer: {ex.Message}",
                PrintJobId = printJobId
            };
        }
    }

    [SupportedOSPlatform("windows")]
    private Task<PrintResult> SendToWindowsPrinterAsync(byte[] data, string printerName, string printJobId, string jobType)
    {
        try
        {
            // Use RawPrinterHelper to send directly to Windows printer
            var result = RawPrinterHelper.SendBytesToPrinter(printerName, data, $"DineHubs_{jobType}_{printJobId}");

            if (result)
            {
                _logger.LogInformation("Print job {PrintJobId} sent to Windows printer {PrinterName}", printJobId, printerName);
                return Task.FromResult(new PrintResult
                {
                    Success = true,
                    Message = "Print job sent successfully",
                    PrintJobId = printJobId
                });
            }

            return Task.FromResult(new PrintResult
            {
                Success = false,
                Message = "Failed to send print job to Windows printer",
                PrintJobId = printJobId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending to Windows printer {PrinterName}", printerName);
            return Task.FromResult(new PrintResult
            {
                Success = false,
                Message = $"Failed to send to printer: {ex.Message}",
                PrintJobId = printJobId
            });
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? GetDefaultPrinter()
    {
        try
        {
            var settings = new PrinterSettings();
            return settings.IsDefaultPrinter ? settings.PrinterName : null;
        }
        catch
        {
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    public List<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        try
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting installed printers");
        }
        return printers;
    }
}

