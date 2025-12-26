namespace OrderManagement.PrintAgent.Services;

public class PrintAgentOptions
{
    public int WebSocketPort { get; set; } = 9100;
    public string ApiBaseUrl { get; set; } = "https://localhost:5001/api/v1";
    public string TenantCode { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int SyncIntervalMinutes { get; set; } = 5;
    public int DefaultPaperWidth { get; set; } = 80;
}

