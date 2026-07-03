using System.IO;

namespace DukomPlinUtility.Services;

public static class LogViewerService
{
    public static string ReadSafe(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            return $"Log nije moguće učitati.\r\n\r\n{ex.Message}";
        }
    }
}
