using System.Globalization;

namespace EBank.Notification.Application;

public static class TemplateRenderer
{
    private static readonly TimeZoneInfo VilniusTz = ResolveVilniusTimeZone();

    private static TimeZoneInfo ResolveVilniusTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Vilnius"); }
        catch { return TimeZoneInfo.Utc; }
    }

    /// <summary>
    /// Renders title and body replacing {placeholders}, formatting amounts to 2 decimals,
    /// dates to Europe/Vilnius time, and returning a masked copy for OTP_REQUESTED (NTF-06 / NTF-NFR-04).
    /// </summary>
    public static (string Title, string DeliveryBody, string StoredBody) Render(
        string eventType,
        string titleTemplate,
        string bodyTemplate,
        IReadOnlyDictionary<string, object?> placeholders)
    {
        var title = ReplaceTokens(titleTemplate, placeholders, maskOtp: false);
        var deliveryBody = ReplaceTokens(bodyTemplate, placeholders, maskOtp: false);
        var storedBody = eventType == "OTP_REQUESTED"
            ? ReplaceTokens(bodyTemplate, placeholders, maskOtp: true)
            : deliveryBody;

        return (title, deliveryBody, storedBody);
    }

    private static string ReplaceTokens(string template, IReadOnlyDictionary<string, object?> placeholders, bool maskOtp)
    {
        var result = template;
        foreach (var (key, rawValue) in placeholders)
        {
            string formatted;
            if (maskOtp && key.Equals("code", StringComparison.OrdinalIgnoreCase))
            {
                formatted = "******";
            }
            else
            {
                formatted = FormatValue(key, rawValue);
            }
            result = result.Replace("{" + key + "}", formatted, StringComparison.Ordinal);
        }
        return result;
    }

    private static string FormatValue(string key, object? value) => value switch
    {
        null => string.Empty,
        decimal d => d.ToString("F2", CultureInfo.InvariantCulture),
        double dbl => dbl.ToString("F2", CultureInfo.InvariantCulture),
        DateTimeOffset dto => TimeZoneInfo.ConvertTime(dto, VilniusTz).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DateTime dt => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc), VilniusTz).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DateOnly dOnly => dOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };
}
