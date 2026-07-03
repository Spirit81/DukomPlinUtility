using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DukomPlinUtility.Helpers;

namespace DukomPlinUtility.Services;

public sealed class NbIotResult
{
    public int Total { get; set; }
    public int Exported { get; set; }
    public int MissingUserCode { get; set; }
    public int DateMismatch { get; set; }
    public string ExportPath { get; set; } = string.Empty;
    public string MissingUserCodePath { get; set; } = string.Empty;
    public string DateMismatchPath { get; set; } = string.Empty;
    public string LogPath { get; set; } = string.Empty;
}

public static class NbIotService
{
    public static NbIotResult Process(string inputFile, string outputFolder, DateTime? expectedDate, bool normalizeCroatian = true)
    {
        Directory.CreateDirectory(outputFolder);
        var lines = File.ReadAllLines(inputFile);
        if (lines.Length == 0) throw new InvalidOperationException("NB-IoT file is empty.");
        var header = SplitLine(lines[0]);
        int idxUser = Find(header, "User Code", "UserCode");
        int idxName = Find(header, "Name");
        int idxReading = Find(header, "Last Reading", "Reading");
        int idxDate = Find(header, "Last Reading Date", "Date");
        int idxMeter = Find(header, "Meter code", "Meter Code", "Meter");
        if (idxUser < 0 || idxReading < 0 || idxMeter < 0) throw new InvalidOperationException("Required NB-IoT columns were not found.");

        var export = new List<string>();
        var missing = new List<string>();
        var dateMismatch = new List<string>();
        int total = 0;
        foreach (var raw in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var p = SplitLine(raw);
            total++;
            string user = Get(p, idxUser);
            string name = normalizeCroatian ? CroatianTextHelper.Normalize(Get(p, idxName)) : Get(p, idxName);
            string readingRaw = Get(p, idxReading);
            string readingWhole = WholePart(readingRaw);
            string date = DateHelper.Normalize(Get(p, idxDate));
            string meter = Get(p, idxMeter);
            export.Add($"{user};{name};;{readingWhole};{date};{meter};");

            var readingValue = SourceRepository.ParseDecimal(readingRaw);
            if (string.IsNullOrWhiteSpace(user) && readingValue > 1m)
                missing.Add($"{meter};{readingWhole};{date};{name}");

            if (expectedDate.HasValue && DateHelper.TryParse(date, out var actual))
            {
                if (Math.Abs((actual.Date - expectedDate.Value.Date).TotalDays) > 2)
                    dateMismatch.Add($"{user};{name};{readingWhole};{date};{meter}");
            }
        }
        var baseName = Path.GetFileNameWithoutExtension(inputFile);
        var exportPath = Path.Combine(outputFolder, baseName + "_nbiot_result.txt");
        var missingPath = Path.Combine(outputFolder, baseName + "_nbiot_missing_usercode.txt");
        var datePath = Path.Combine(outputFolder, baseName + "_nbiot_date_mismatch.txt");
        var logPath = Path.Combine(outputFolder, baseName + "_nbiot.log");
        File.WriteAllLines(exportPath, export);
        File.WriteAllLines(missingPath, missing);
        File.WriteAllLines(datePath, dateMismatch);
        File.WriteAllLines(logPath, new []{
            "DUKOM PLIN Utility - NB-IoT", $"Date: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            $"Input: {inputFile}", $"Total records: {total}", $"Exported: {export.Count}",
            $"Reading > 1 without User Code: {missing.Count}", $"Date mismatch: {dateMismatch.Count}"
        });
        return new NbIotResult{Total=total, Exported=export.Count, MissingUserCode=missing.Count, DateMismatch=dateMismatch.Count, ExportPath=exportPath, MissingUserCodePath=missingPath, DateMismatchPath=datePath, LogPath=logPath};
    }

    private static string[] SplitLine(string line) => line.Split('\t').Length > 1 ? line.Split('\t') : line.Split(';');
    private static string Get(string[] p, int idx) => idx >= 0 && idx < p.Length ? p[idx].Trim().Trim('"') : string.Empty;
    private static int Find(string[] h, params string[] names) => Array.FindIndex(h, x => names.Any(n => string.Equals(x.Trim(), n, StringComparison.OrdinalIgnoreCase)));
    private static string WholePart(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return "0";
        var s = v.Trim().Trim('"');
        var m = Regex.Match(s, @"^-?\d+");
        return m.Success ? m.Value : "0";
    }
}
