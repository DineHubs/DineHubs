using System.Text.Json.Serialization;

namespace OrderManagement.PrintAgent.Models;

public class PrintJob
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("printerType")]
    public int PrinterType { get; set; }

    [JsonPropertyName("printerName")]
    public string? PrinterName { get; set; }

    [JsonPropertyName("data")]
    public PrintJobData? Data { get; set; }
}

public class PrintJobData
{
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("tableNumber")]
    public string? TableNumber { get; set; }

    [JsonPropertyName("isTakeAway")]
    public bool IsTakeAway { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("lines")]
    public List<PrintJobLine> Lines { get; set; } = [];

    [JsonPropertyName("payment")]
    public PrintJobPayment? Payment { get; set; }
}

public class PrintJobLine
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("lineTotal")]
    public decimal LineTotal { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class PrintJobPayment
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("change")]
    public decimal Change { get; set; }
}

public class PrintResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("printJobId")]
    public string? PrintJobId { get; set; }
}

