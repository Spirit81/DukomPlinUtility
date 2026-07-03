using System.IO;
using System.Text.Json;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.Services;

public static class SettingsService
{
    private static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DukomPlinUtility");
    private static readonly string FilePath = Path.Combine(Dir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new AppSettings();
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            try { File.Copy(FilePath, FilePath + ".bad", true); } catch { }
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Dir);
        settings.WindowLeft = Safe(settings.WindowLeft, 80);
        settings.WindowTop = Safe(settings.WindowTop, 60);
        settings.WindowWidth = Safe(settings.WindowWidth, 1800);
        settings.WindowHeight = Safe(settings.WindowHeight, 950);
        var opt = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, opt));
    }

    private static double Safe(double value, double fallback)
        => double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? fallback : value;
}
