using System.IO;
using System.Xml.Linq;

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
            if (!string.IsNullOrEmpty(broj)) map[broj] = stanje ?? "";
        }
        var matched = new HashSet<string>();
        var result = new List<string>();
        var sourceLines = File.ReadAllLines(sourceFile);
        foreach (var line in sourceLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var p = line.Split(';');
            if (p.Length < 6) continue;
            var broj = p[5].Trim();
            if (!map.TryGetValue(broj, out var stanje)) continue;
            matched.Add(broj);
            var rfid = p.Length > 6 ? p[6].Trim() : string.Empty;
            result.Add($"{p[0].Trim()};{p[1].Trim()};{p[2].Trim()};{stanje};;{broj};{rfid}");
        }
        var missing = map.Keys.Where(k => !matched.Contains(k)).Select(k => $"{k};{map[k]}").ToList();
        var outPath = Path.Combine(outputFolder, "output.txt");
        var resultPath = Path.Combine(outputFolder, "result.txt");
        var missingPath = Path.Combine(outputFolder, "missing.txt");
        var logPath = Path.Combine(outputFolder, "walkby.log");
        File.WriteAllLines(outPath, map.Select(kv => $"{kv.Key};{kv.Value}"));
        File.WriteAllLines(resultPath, result);
        File.WriteAllLines(missingPath, missing);
        File.WriteAllLines(logPath, new []{
            $"DUKOM PLIN Utility - Holosys WalkBy", $"Date: {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            $"XML: {xmlFile}", $"Source: {sourceFile}", $"Output: {outputFolder}",
            $"XML records: {map.Count}", $"Matched: {matched.Count}", $"Missing: {missing.Count}"
        });
        return new WalkByResult{XmlCount=map.Count, Matched=matched.Count, Missing=missing.Count, OutputPath=outPath, ResultPath=resultPath, MissingPath=missingPath, LogPath=logPath};
    }
}
