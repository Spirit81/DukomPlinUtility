using System.IO;
using System.Windows;

namespace DukomPlinUtility.Helpers;

public static class DragDropHelper
{
    public static void SetFileDropEffect(DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    public static string[] GetDroppedFiles(DragEventArgs e)
    {
        return e.Data.GetData(DataFormats.FileDrop) as string[] ?? Array.Empty<string>();
    }

    public static string? FirstFileByExtension(DragEventArgs e, string extension)
    {
        return GetDroppedFiles(e)
            .FirstOrDefault(path => File.Exists(path) &&
                                    Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    public static string? FirstFile(DragEventArgs e)
    {
        return GetDroppedFiles(e).FirstOrDefault(File.Exists);
    }

    public static string? DroppedDirectoryOrParent(DragEventArgs e)
    {
        var first = GetDroppedFiles(e).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first)) return null;

        if (Directory.Exists(first)) return first;
        return File.Exists(first) ? Path.GetDirectoryName(first) : null;
    }
}
