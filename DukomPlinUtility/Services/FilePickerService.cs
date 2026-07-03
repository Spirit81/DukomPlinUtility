using Microsoft.Win32;

namespace DukomPlinUtility.Services;

public static class FilePickerService
{
    public static string? PickXmlFile()
        => PickFile("XML files (*.xml)|*.xml|All files (*.*)|*.*");

    public static string? PickTextFile()
        => PickFile("TXT files (*.txt)|*.txt|All files (*.*)|*.*");

    public static string? PickNbIotFile()
        => PickFile("Export files (*.xls;*.xlsx;*.txt;*.csv)|*.xls;*.xlsx;*.txt;*.csv|All files (*.*)|*.*");

    public static IReadOnlyList<string> PickZgradeFiles()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*",
            Multiselect = true
        };

        return dialog.ShowDialog() == true
            ? dialog.FileNames
            : Array.Empty<string>();
    }

    public static string? PickOutputFolder()
    {
        var dialog = new OpenFolderDialog();
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public static string? PickLogFile()
        => PickFile("Log/TXT files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*");

    private static string? PickFile(string filter)
    {
        var dialog = new OpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
