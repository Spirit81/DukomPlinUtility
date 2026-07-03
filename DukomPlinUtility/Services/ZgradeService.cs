using System.IO;
using System.Text.RegularExpressions;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.Services;

public sealed class ZgradeResult
{
    public int Total { get; set; }
    public int Issues { get; set; }
    public int Lower { get; set; }
    public int Duplicates { get; set; }
    public string Folder { get; set; } = string.Empty;
    public string MergedPath { get; set; } = string.Empty;
    public string MismatchPath { get; set; } = string.Empty;
    public string LowerPath { get; set; } = string.Empty;
    public string DuplicatesPath { get; set; } = string.Empty;
    public string LogPath { get; set; } = string.Empty;
    public List<ZgradeIssue> IssueList { get; set; } = new();
}

public static class ZgradeService
{
    public static ZgradeResult Process(IEnumerable<string> files, string sourceFile, string outputParentFolder)
    {
        var outFolder = Path.Combine(outputParentFolder, "Zgrade_obrade");
        Directory.CreateDirectory(outFolder);
        var merged = new List<string>();
        foreach (var f in files.Where(File.Exists)) merged.AddRange(File.ReadAllLines(f));

        var byUser = SourceRepository.LoadByUser(sourceFile);
        var issues = new List<ZgradeIssue>();
        var fullLineCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var pairCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var userMeters = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var parsed = new List<(string Raw, string User, string Meter, decimal NewReading, string NewReadingText)>();

        foreach (var raw in merged.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var normalizedRaw = Regex.Replace(raw.Trim(), @"\s+", " ");
            fullLineCount[normalizedRaw] = fullLineCount.GetValueOrDefault(normalizedRaw) + 1;
            var parts = Regex.Split(raw.Trim(), @"\s+").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (parts.Length < 2) continue;
            var user = parts[0];
            var meter = parts[1];
            var readingText = parts.Length >= 5 ? parts[4] : (parts.Length >= 4 ? parts[3] : "0");
            var newReading = SourceRepository.ParseDecimal(readingText);
            parsed.Add((normalizedRaw, user, meter, newReading, readingText));
            var pairKey = user + "|" + meter;
            pairCount[pairKey] = pairCount.GetValueOrDefault(pairKey) + 1;
            if (!userMeters.TryGetValue(user, out var set)) userMeters[user] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            set.Add(meter);
        }

        foreach (var rec in parsed)
        {
            if (fullLineCount[rec.Raw] > 1)
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, NewReading=rec.NewReadingText, Status="Duplicate line", StatusLevel="Error"});
            if (pairCount[rec.User + "|" + rec.Meter] > 1)
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, NewReading=rec.NewReadingText, Status="Duplicate UserCode+Meter", StatusLevel="Error"});
            if (userMeters.TryGetValue(rec.User, out var meters) && meters.Count > 1)
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, NewReading=rec.NewReadingText, Status="User Code has multiple meters", StatusLevel="Warning"});

            if (!byUser.TryGetValue(rec.User, out var src))
            {
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, NewReading=rec.NewReadingText, Status="User Code not found", StatusLevel="Error"});
                continue;
            }
            if (!string.Equals(src.Meter, rec.Meter, StringComparison.OrdinalIgnoreCase))
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, SourceReading=src.SourceValue, NewReading=rec.NewReadingText, Status=$"Meter mismatch, source meter: {src.Meter}", StatusLevel="Error"});
            var sourceReading = SourceRepository.ParseDecimal(src.SourceValue);
            if (rec.NewReading < sourceReading)
                issues.Add(new ZgradeIssue{UserCode=rec.User, Meter=rec.Meter, SourceReading=src.SourceValue, NewReading=rec.NewReadingText, Difference=(rec.NewReading-sourceReading).ToString(), Status="Lower reading", StatusLevel="Error"});
        }

        var mergedPath = Path.Combine(outFolder, "zgrade_merged.txt");
        var mismatchPath = Path.Combine(outFolder, "zgrade_mismatch.txt");
        var lowerPath = Path.Combine(outFolder, "zgrade_lower_reading.txt");
        var dupPath = Path.Combine(outFolder, "zgrade_duplicates.txt");
        var logPath = Path.Combine(outFolder, "zgrade.log");
        File.WriteAllLines(mergedPath, merged);
        File.WriteAllLines(mismatchPath, issues.Select(i => $"{i.UserCode};{i.Meter};{i.SourceReading};{i.NewReading};{i.Difference};{i.Status}"));
        File.WriteAllLines(lowerPath, issues.Where(i => i.Status == "Lower reading").Select(i => $"{i.UserCode};{i.Meter};{i.SourceReading};{i.NewReading};{i.Difference}"));
        File.WriteAllLines(dupPath, issues.Where(i => i.Status.Contains("Duplicate") || i.Status.Contains("multiple")).Select(i => $"{i.UserCode};{i.Meter};{i.Status}"));
        File.WriteAllLines(logPath, new []{
            "DUKOM PLIN Utility - Zgrade", $"Date: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            $"Input files: {string.Join(", ", files.Select(Path.GetFileName))}", $"Source: {sourceFile}",
            $"Merged rows: {parsed.Count}", $"Issues: {issues.Count}",
            $"Lower readings: {issues.Count(i => i.Status == "Lower reading")}",
            $"Duplicates/warnings: {issues.Count(i => i.Status.Contains("Duplicate") || i.Status.Contains("multiple"))}"
        });
        return new ZgradeResult{Total=parsed.Count, Issues=issues.Count, Lower=issues.Count(i=>i.Status=="Lower reading"), Duplicates=issues.Count(i=>i.Status.Contains("Duplicate")||i.Status.Contains("multiple")), Folder=outFolder, MergedPath=mergedPath, MismatchPath=mismatchPath, LowerPath=lowerPath, DuplicatesPath=dupPath, LogPath=logPath, IssueList=issues};
    }
}
