using System.IO;
using System.Text.RegularExpressions;
using DukomPlinUtility.Helpers;
using DukomPlinUtility.Models;

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
    public List<ValidationItem> Preview { get; set; } = new();
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

        if (idxUser < 0 || idxReading < 0 || idxMeter < 0)
            throw new InvalidOperationException("Required NB-IoT columns were not found.");

        var export = new List<string>();
        var missing = new List<string>();
        var dateMismatch = new List<string>();
        var preview = new List<ValidationItem>();
        int total = 0;

        foreach (var raw in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var p = SplitLine(raw);
            total++;

            string user = Get(p, idxUser);
            string originalName = Get(p, idxName);
            string name = normalizeCroatian ? CroatianTextHelper.Normalize(originalName) : originalName;
            string readingRaw = Get(p, idxReading);
            string readingWhole = WholePart(readingRaw);
            string date = DateHelper.Normalize(Get(p, idxDate));
            string meter = Get(p, idxMeter);

            export.Add($"{user};{name};;{readingWhole};{date};{meter};");

            var readingValue = SourceRepository.ParseDecimal(readingRaw);
            var hasMissingUserCode = string.IsNullOrWhiteSpace(user) && readingValue > 1m;
            var hasDateMismatch = false;

            if (hasMissingUserCode)
                missing.Add($"{meter};{readingWhole};{date};{name}");

            if (expectedDate.HasValue && DateHelper.TryParse(date, out var actual))
            {
                if (Math.Abs((actual.Date - expectedDate.Value.Date).TotalDays) > 2)
                {
                    hasDateMismatch = true;
                    dateMismatch.Add($"{user};{name};{readingWhole};{date};{meter}");
                }
            }

            var status = "OK";
            var level = "OK";
            var messages = new List<string>();

            if (hasMissingUserCode)
            {
                status = "Missing UserCode";
                level = "Error";
                messages.Add("Reading > 1, a User Code je prazan");
            }

            if (hasDateMismatch)
            {
                if (level != "Error")
                {
                    status = "Date mismatch";
                    level = "Warning";
                }
                else
                {
                    status += " + Date mismatch";
                }
                messages.Add("Datum očitanja odstupa više od ±2 dana");
            }

            if (messages.Count == 0)
            {
                messages.Add("Spremno za export");
            }

            preview.Add(new ValidationItem
            {
                UserCode = user,
                Name = name,
                Meter = meter,
                Reading = readingWhole,
                Date = date,
                Status = status,
                StatusLevel = level,
                Message = string.Join("; ", messages)
            });
        }

        var baseName = Path.GetFileNameWithoutExtension(inputFile);
        var exportPath = Path.Combine(outputFolder, baseName + "_nbiot_result.txt");
        var missingPath = Path.Combine(outputFolder, baseName + "_nbiot_missing_usercode.txt");
        var datePath = Path.Combine(outputFolder, baseName + "_nbiot_date_mismatch.txt");
        var logPath = Path.Combine(outputFolder, baseName + "_nbiot.log");

        File.WriteAllLines(exportPath, export);
        File.WriteAllLines(missingPath, missing);
        File.WriteAllLines(datePath, dateMismatch);
        File.WriteAllLines(logPath, new[]
        {
            "DUKOM PLIN Utility - NB-IoT",
            $"Date: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            $"Input: {inputFile}",
            $"Total records: {total}",
            $"Exported: {export.Count}",
            $"Reading > 1 without User Code: {missing.Count}",
            $"Date mismatch: {dateMismatch.Count}"
        });

        return new NbIotResult
        {
            Total = total,
            Exported = export.Count,
            MissingUserCode = missing.Count,
            DateMismatch = dateMismatch.Count,
            ExportPath = exportPath,
            MissingUserCodePath = missingPath,
            DateMismatchPath = datePath,
            LogPath = logPath,
            Preview = preview
        };
    }

    private static string[] SplitLine(string line) => line.Split('\t').Length > 1 ? line.Split('\t') : line.Split(';');
    private static string Get(string[] p, int idx) => idx >= 0 && idx < p.Length ? p[idx].Trim().Trim('"') : string.Empty;
    private static int Find(string[] h, params string[] names) => Array.FindIndex(h, x => names.Any(n => string.Equals(x.Trim(), n, StringComparison.OrdinalIgnoreCase)));

    private static string WholePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "0";
        var s = value.Trim().Trim('"');
        var match = Regex.Match(s, @"^-?\d+");
        return match.Success ? match.Value : "0";
    }
}
