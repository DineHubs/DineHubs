using Ganss.Xss;

namespace OrderManagement.Infrastructure.Security;

public interface IInputSanitizer
{
    string SanitizeHtml(string? input);
    string SanitizeForSql(string? input);
}

public sealed class InputSanitizer : IInputSanitizer
{
    private readonly HtmlSanitizer _htmlSanitizer;

    public InputSanitizer()
    {
        _htmlSanitizer = new HtmlSanitizer();
        // Allow only safe HTML tags and attributes
        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedAttributes.Clear();
    }

    public string SanitizeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return _htmlSanitizer.Sanitize(input);
    }

    public string SanitizeForSql(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // EF Core uses parameterized queries, but we can still remove potentially dangerous characters
        // This is a defense-in-depth measure
        return input
            .Replace("'", "''") // Escape single quotes (though EF Core handles this)
            .Replace(";", "") // Remove semicolons that could be used for SQL injection
            .Replace("--", "") // Remove SQL comment markers
            .Replace("/*", "") // Remove SQL comment start
            .Replace("*/", ""); // Remove SQL comment end
    }
}

