using System.Globalization;
using System.IO;

namespace DukomPlinUtility.Services;

public sealed class SourceRecord
{
    public string UserCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Omm { get; init; } = string.Empty;
    public string SourceValue { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Meter { get; init; } = string.Empty;
    public string Rfid { get; init; } = string.Empty;
}

public static class SourceRepository
{
    public static Dictionary<string, SourceRecord> LoadByUser(string sourceFile)
    {
        var dict = new Dictionary<string, SourceRecord>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(sourceFile)) return dict;
        foreach (var line in File.ReadLines(sourceFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split(';');
            if (p.Length < 6) continue;
            var rec = new SourceRecord
            {
                UserCode = p[0].Trim(),
                Name = p.Length > 1 ? p[1].Trim() : "",
                Omm = p.Length > 2 ? p[2].Trim() : "",
                SourceValue = p.Length > 3 ? p[3].Trim() : "",
                Date = p.Length > 4 ? p[4].Trim() : "",
                Meter = p.Length > 5 ? p[5].Trim() : "",
                Rfid = p.Length > 6 ? p[6].Trim() : ""
            };
            if (!string.IsNullOrWhiteSpace(rec.UserCode)) dict[rec.UserCode] = rec;
        }
        return dict;
    }

    public static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0m;
        var s = value.Trim().Replace(" ", "").Replace(',', '.');
        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d);
        return d;
    }
}
