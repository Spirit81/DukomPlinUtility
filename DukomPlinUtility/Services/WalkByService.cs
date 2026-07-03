using System.IO;
using System.Xml.Linq;
using DukomPlinUtility.Helpers;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.Services;

public sealed class WalkByResult
{
    public int XmlCount { get; set; }
    public int Matched { get; set; }
    public int Missing { get; set; }
    public string ResultPath { get; set; } = string.Empty;
    public string MissingPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string LogPath { get; set; } = string.Empty;
    public List<ValidationItem> Preview { get; set; } = new();
}

public static class WalkByService
{
    public static WalkByResult Process(string xmlFile, string sourceFile, string outputFolder)
    {
        Directory.CreateDirectory(outputFolder);

        var doc = XDocument.Load(xmlFile);
        var map = new Dictionary<string, string>();
        foreach (var m in doc.Descendants("Mjerilo"))
        {
            var broj = m.Element("Broj")?.Value.Trim();
            var stanje = m.Element("Stanje")?.Value.Trim();
            if (!string.IsNullOrEmpty(broj))
            {
                map[broj] = stanje ?? string.Empty;
            }
        }

        var matched = new HashSet<string>();
        var resultLines = new List<string>();
        var preview = new List<ValidationItem>();
        var sourceLines = File.ReadAllLines(sourceFile);

        foreach (var line in sourceLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split(';');
            if (p.Length < 6) continue;

            var userCode = p[0].Trim();
            var name = p.Length > 1 ? p[1].Trim() : string.Empty;
            var previousReading = p.Length > 3 ? p[3].Trim() : string.Empty;
            var sourceDate = p.Length > 4 ? DateHelper.Normalize(p[4].Trim()) : string.Empty;
            var broj = p[5].Trim();

            if (!map.TryGetValue(broj, out var stanje)) continue;

            matched.Add(broj);
            var rfid = p.Length > 6 ? p[6].Trim() : string.Empty;
            resultLines.Add($"{userCode};{name};{p[2].Trim()};{stanje};;{broj};{rfid}");

            preview.Add(new ValidationItem
            {
                UserCode = userCode,
                Name = name,
                Meter = broj,
                Reading = stanje,
                PreviousReading = previousReading,
                Date = sourceDate,
                Status = "OK",
                StatusLevel = "OK",
                Message = "Uspješno upareno"
            });
        }

        var missing = map.Keys
            .Where(k => !matched.Contains(k))
            .Select(k => $"{k};{map[k]}")
            .ToList();

        foreach (var line in missing)
        {
            var p = line.Split(';');
            preview.Add(new ValidationItem
            {
                Meter = p[0],
                Reading = p.Length > 1 ? p[1] : string.Empty,
                Status = "Missing",
                StatusLevel = "Error",
                Message = "Brojilo iz XML-a nije pronađeno u source fajlu"
            });
        }

        var outPath = Path.Combine(outputFolder, "output.txt");
        var resultPath = Path.Combine(outputFolder, "result.txt");
        var missingPath = Path.Combine(outputFolder, "missing.txt");
        var logPath = Path.Combine(outputFolder, "walkby.log");

        File.WriteAllLines(outPath, map.Select(kv => $"{kv.Key};{kv.Value}"));
        File.WriteAllLines(resultPath, resultLines);
        File.WriteAllLines(missingPath, missing);
        File.WriteAllLines(logPath, new[]
        {
            "DUKOM PLIN Utility - Holosys WalkBy",
            $"Date: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            $"XML: {xmlFile}",
            $"Source: {sourceFile}",
            $"Output: {outputFolder}",
            $"XML records: {map.Count}",
            $"Matched: {matched.Count}",
            $"Missing: {missing.Count}"
        });

        return new WalkByResult
        {
            XmlCount = map.Count,
            Matched = matched.Count,
            Missing = missing.Count,
            OutputPath = outPath,
            ResultPath = resultPath,
            MissingPath = missingPath,
            LogPath = logPath,
            Preview = preview
        };
    }
}
