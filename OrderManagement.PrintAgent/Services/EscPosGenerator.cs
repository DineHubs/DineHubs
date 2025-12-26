using System.Text;
using OrderManagement.PrintAgent.Models;

namespace OrderManagement.PrintAgent.Services;

/// <summary>
/// Generates ESC/POS commands for thermal printers
/// </summary>
public class EscPosGenerator
{
    // ESC/POS Commands
    private static readonly byte[] Initialize = [0x1B, 0x40]; // ESC @
    private static readonly byte[] CutPaper = [0x1D, 0x56, 0x00]; // GS V 0 (full cut)
    private static readonly byte[] CutPaperPartial = [0x1D, 0x56, 0x01]; // GS V 1 (partial cut)
    private static readonly byte[] AlignLeft = [0x1B, 0x61, 0x00]; // ESC a 0
    private static readonly byte[] AlignCenter = [0x1B, 0x61, 0x01]; // ESC a 1
    private static readonly byte[] AlignRight = [0x1B, 0x61, 0x02]; // ESC a 2
    private static readonly byte[] BoldOn = [0x1B, 0x45, 0x01]; // ESC E 1
    private static readonly byte[] BoldOff = [0x1B, 0x45, 0x00]; // ESC E 0
    private static readonly byte[] DoubleHeight = [0x1B, 0x21, 0x10]; // ESC ! 16
    private static readonly byte[] DoubleWidth = [0x1B, 0x21, 0x20]; // ESC ! 32
    private static readonly byte[] DoubleSize = [0x1B, 0x21, 0x30]; // ESC ! 48
    private static readonly byte[] NormalSize = [0x1B, 0x21, 0x00]; // ESC ! 0
    private static readonly byte[] LineFeed = [0x0A]; // LF
    private static readonly byte[] OpenDrawer = [0x1B, 0x70, 0x00, 0x19, 0xFA]; // ESC p 0 25 250

    private readonly int _paperWidth;
    private readonly int _charsPerLine;

    public EscPosGenerator(int paperWidth = 80)
    {
        _paperWidth = paperWidth;
        // 80mm paper = ~48 chars, 58mm paper = ~32 chars (at standard font)
        _charsPerLine = paperWidth == 80 ? 48 : 32;
    }

    public byte[] GenerateReceipt(PrintJobData data)
    {
        using var ms = new MemoryStream();

        // Initialize printer
        ms.Write(Initialize);

        // Header - centered
        ms.Write(AlignCenter);
        ms.Write(BoldOn);
        ms.Write(DoubleSize);
        WriteLine(ms, "DineHubs");
        ms.Write(NormalSize);
        ms.Write(BoldOff);
        WriteLine(ms, "Restaurant POS");
        WriteLine(ms, "");

        // Order info
        ms.Write(AlignLeft);
        ms.Write(BoldOn);
        WriteLine(ms, $"Order: {data.OrderNumber}");
        ms.Write(BoldOff);

        if (data.IsTakeAway)
        {
            WriteLine(ms, "Type: TAKEAWAY");
        }
        else
        {
            WriteLine(ms, $"Table: {data.TableNumber}");
        }

        if (!string.IsNullOrEmpty(data.CreatedAt))
        {
            var date = DateTime.TryParse(data.CreatedAt, out var dt)
                ? dt.ToString("dd/MM/yyyy HH:mm")
                : data.CreatedAt;
            WriteLine(ms, $"Date: {date}");
        }

        // Divider
        WriteDivider(ms);

        // Items header
        WriteLineColumns(ms, "Item", "Qty", "Price", "Total");
        WriteDivider(ms, '-');

        // Items
        foreach (var line in data.Lines)
        {
            // Item name (may wrap)
            var name = TruncateOrPad(line.Name, _charsPerLine - 20);
            var qty = line.Quantity.ToString().PadLeft(3);
            var price = line.UnitPrice.ToString("F2").PadLeft(7);
            var total = line.LineTotal.ToString("F2").PadLeft(8);

            WriteLine(ms, $"{name}{qty}{price}{total}");

            if (!string.IsNullOrEmpty(line.Notes))
            {
                WriteLine(ms, $"  -> {line.Notes}");
            }
        }

        // Divider
        WriteDivider(ms);

        // Total
        ms.Write(BoldOn);
        ms.Write(DoubleHeight);
        WriteLineRight(ms, $"TOTAL: RM {data.Total:F2}");
        ms.Write(NormalSize);
        ms.Write(BoldOff);

        // Payment info
        if (data.Payment != null)
        {
            WriteLine(ms, "");
            WriteLine(ms, $"Payment: {data.Payment.Provider}");
            WriteLine(ms, $"Paid: RM {data.Payment.Amount:F2}");
            if (data.Payment.Change > 0)
            {
                WriteLine(ms, $"Change: RM {data.Payment.Change:F2}");
            }
        }

        // Footer
        WriteLine(ms, "");
        WriteDivider(ms);
        ms.Write(AlignCenter);
        WriteLine(ms, "Thank you for dining with us!");
        WriteLine(ms, "Please come again");
        WriteLine(ms, "");
        WriteLine(ms, "");

        // Cut paper
        ms.Write(CutPaperPartial);

        return ms.ToArray();
    }

    public byte[] GenerateKitchenTicket(PrintJobData data)
    {
        using var ms = new MemoryStream();

        // Initialize printer
        ms.Write(Initialize);

        // Header - centered, large
        ms.Write(AlignCenter);
        ms.Write(BoldOn);
        ms.Write(DoubleSize);
        WriteLine(ms, "KITCHEN ORDER");
        ms.Write(NormalSize);
        WriteLine(ms, "");

        // Order number - large
        ms.Write(DoubleSize);
        WriteLine(ms, data.OrderNumber);
        ms.Write(NormalSize);
        ms.Write(BoldOff);
        WriteLine(ms, "");

        // Table/Takeaway
        ms.Write(BoldOn);
        if (data.IsTakeAway)
        {
            ms.Write(DoubleSize);
            WriteLine(ms, "*** TAKEAWAY ***");
            ms.Write(NormalSize);
        }
        else
        {
            ms.Write(DoubleHeight);
            WriteLine(ms, $"TABLE: {data.TableNumber}");
            ms.Write(NormalSize);
        }
        ms.Write(BoldOff);

        // Time
        ms.Write(AlignLeft);
        if (!string.IsNullOrEmpty(data.CreatedAt))
        {
            var date = DateTime.TryParse(data.CreatedAt, out var dt)
                ? dt.ToString("HH:mm")
                : data.CreatedAt;
            WriteLine(ms, $"Time: {date}");
        }

        // Divider
        WriteDivider(ms, '=');
        WriteLine(ms, "");

        // Items - large and clear
        foreach (var line in data.Lines)
        {
            ms.Write(BoldOn);
            ms.Write(DoubleHeight);
            WriteLine(ms, $"{line.Quantity}x {line.Name}");
            ms.Write(NormalSize);
            ms.Write(BoldOff);

            if (!string.IsNullOrEmpty(line.Notes))
            {
                WriteLine(ms, $"   -> {line.Notes}");
            }
            WriteLine(ms, "");
        }

        // Footer
        WriteDivider(ms, '=');
        ms.Write(AlignCenter);
        WriteLine(ms, "*** KITCHEN COPY ***");
        WriteLine(ms, "");
        WriteLine(ms, "");

        // Cut paper
        ms.Write(CutPaperPartial);

        return ms.ToArray();
    }

    public byte[] GenerateTestPage()
    {
        using var ms = new MemoryStream();

        ms.Write(Initialize);
        ms.Write(AlignCenter);
        ms.Write(BoldOn);
        ms.Write(DoubleSize);
        WriteLine(ms, "PRINT TEST");
        ms.Write(NormalSize);
        ms.Write(BoldOff);
        WriteLine(ms, "");
        WriteLine(ms, "DineHubs Print Agent");
        WriteLine(ms, $"Paper Width: {_paperWidth}mm");
        WriteLine(ms, $"Chars/Line: {_charsPerLine}");
        WriteLine(ms, $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLine(ms, "");
        WriteDivider(ms);
        WriteLine(ms, "Printer is working correctly!");
        WriteLine(ms, "");
        WriteLine(ms, "");
        ms.Write(CutPaperPartial);

        return ms.ToArray();
    }

    public byte[] GetOpenDrawerCommand() => OpenDrawer;

    private void WriteLine(MemoryStream ms, string text)
    {
        var bytes = Encoding.GetEncoding("IBM437").GetBytes(text);
        ms.Write(bytes);
        ms.Write(LineFeed);
    }

    private void WriteLineRight(MemoryStream ms, string text)
    {
        var padding = Math.Max(0, _charsPerLine - text.Length);
        WriteLine(ms, new string(' ', padding) + text);
    }

    private void WriteLineColumns(MemoryStream ms, string col1, string col2, string col3, string col4)
    {
        var c1Width = _charsPerLine - 18;
        var formatted = $"{TruncateOrPad(col1, c1Width)}{col2.PadLeft(3)}{col3.PadLeft(7)}{col4.PadLeft(8)}";
        WriteLine(ms, formatted);
    }

    private void WriteDivider(MemoryStream ms, char c = '-')
    {
        WriteLine(ms, new string(c, _charsPerLine));
    }

    private static string TruncateOrPad(string text, int length)
    {
        if (text.Length > length)
            return text[..(length - 1)] + ".";
        return text.PadRight(length);
    }
}

