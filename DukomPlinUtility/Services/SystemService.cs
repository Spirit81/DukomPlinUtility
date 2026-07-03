using System.Diagnostics;
using System.IO;

namespace DukomPlinUtility.Services;

public static class SystemService
{
    public static void OpenFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!Directory.Exists(path)) return;

        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }
}
