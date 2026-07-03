using System.Globalization;

namespace DukomPlinUtility.Helpers;

public static class DateHelper
{
    private static readonly string[] Formats =
    {
        "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yyyy HH:mm:ss", "d.M.yyyy H:mm:ss", "dd.MM.yyyy HH:mm", "d.M.yyyy H:mm",
        "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss",
        "MM/dd/yyyy", "M/d/yyyy", "MM/dd/yyyy HH:mm:ss", "M/d/yyyy H:mm:ss",
        "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy/MM/dd", "yyyy/MM/dd HH:mm:ss"
    };

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim();
        if (DateTime.TryParseExact(s, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        if (DateTime.TryParse(s, new CultureInfo("hr-HR"), DateTimeStyles.None, out dt))
            return dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        return s;
    }

    public static bool TryParse(string? input, out DateTime date)
    {
        date = default;
        var normalized = Normalize(input);
        return DateTime.TryParseExact(normalized, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
}
