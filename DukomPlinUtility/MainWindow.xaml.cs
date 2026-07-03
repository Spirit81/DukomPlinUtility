using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Models;
using DukomPlinUtility.Services;

namespace DukomPlinUtility;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly ObservableCollection<string> _zgradeFiles = new();

    public MainWindow()
    {
        InitializeComponent();

        _settings = SettingsService.Load();
        WindowSettingsService.Apply(this, _settings);

        WireViewEvents();
        LoadSettingsToUi();

        ViewZgrade.FilesSource = _zgradeFiles;

        UpdateSourceViews();
        ShowView(AppModuleExtensions.FromString(_settings.LastModule));

        Closing += (_, _) => SaveSettingsFromUi();
    }

    private void WireViewEvents()
    {
        ViewDashboard.WalkByRequested += (_, _) => ShowView(AppModule.WalkBy);
        ViewDashboard.NbIotRequested += (_, _) => ShowView(AppModule.NbIot);
        ViewDashboard.ZgradeRequested += (_, _) => ShowView(AppModule.Zgrade);

        ViewWalkBy.BrowseXmlRequested += (_, _) => BrowseXml();
        ViewWalkBy.BrowseOutputRequested += (_, _) => BrowseOutput();
        ViewWalkBy.OpenSettingsRequested += (_, _) => ShowView(AppModule.Settings);
        ViewWalkBy.RunRequested += (_, _) => RunWalkBy();
        ViewWalkBy.OpenOutputRequested += (_, _) => OpenOutputFolder();

        ViewNbIot.BrowseNbIotRequested += (_, _) => BrowseNbIot();
        ViewNbIot.RunRequested += (_, _) => RunNbIot();
        ViewNbIot.OpenOutputRequested += (_, _) => OpenOutputFolder();

        ViewZgrade.BrowseFilesRequested += (_, _) => BrowseZgrade();
        ViewZgrade.ClearRequested += (_, _) => _zgradeFiles.Clear();
        ViewZgrade.RunRequested += (_, _) => RunZgrade();
        ViewZgrade.FileDropped += (_, file) => AddZgradeFile(file);

        ViewLogs.OpenLogRequested += (_, _) => OpenLog();

        ViewSettings.BrowseSourceRequested += (_, _) => BrowseSource();
        ViewSettings.SaveRequested += (_, _) => SaveSettings();
    }

    private void LoadSettingsToUi()
    {
        ViewSettings.SharedSourceFile = _settings.SharedSourceFile;
        ViewWalkBy.OutputFolder = _settings.LastOutputFolder;
        ViewNbIot.OutputFolder = _settings.LastOutputFolder;
        ViewNbIot.ExpectedDate = DateTime.Today;
    }

    private void SaveSettingsFromUi()
    {
        try
        {
            WindowSettingsService.Capture(this, _settings);
            _settings.SharedSourceFile = ViewSettings.SharedSourceFile.Trim();
            _settings.LastOutputFolder = FirstNonEmpty(ViewWalkBy.OutputFolder, ViewNbIot.OutputFolder, _settings.LastOutputFolder);
            SettingsService.Save(_settings);
        }
        catch
        {
            // Settings must never crash the application.
        }
    }

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    #region Navigation

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag })
        {
            ShowView(AppModuleExtensions.FromString(tag));
        }
    }

    private void ShowView(AppModule module)
    {
        HideAllViews();

        switch (module)
        {
            case AppModule.WalkBy:
                ViewWalkBy.Visibility = Visibility.Visible;
                break;
            case AppModule.NbIot:
                ViewNbIot.Visibility = Visibility.Visible;
                break;
            case AppModule.Zgrade:
                ViewZgrade.Visibility = Visibility.Visible;
                break;
            case AppModule.Logs:
                ViewLogs.Visibility = Visibility.Visible;
                break;
            case AppModule.Settings:
                ViewSettings.Visibility = Visibility.Visible;
                break;
            case AppModule.About:
                ViewAbout.Visibility = Visibility.Visible;
                break;
            default:
                ViewDashboard.Visibility = Visibility.Visible;
                module = AppModule.Dashboard;
                break;
        }

        _settings.LastModule = module.ToString();
        UpdateSourceViews();
    }

    private void HideAllViews()
    {
        foreach (var view in new FrameworkElement[]
                 {
                     ViewDashboard,
                     ViewWalkBy,
                     ViewNbIot,
                     ViewZgrade,
                     ViewLogs,
                     ViewSettings,
                     ViewAbout
                 })
        {
            view.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateSourceViews()
    {
        var source = string.IsNullOrWhiteSpace(ViewSettings.SharedSourceFile)
            ? "Source nije postavljen"
            : ViewSettings.SharedSourceFile;

        ViewDashboard.SetSourceText(source);
        ViewWalkBy.SetSourceText(source);
    }

    #endregion

    #region Browse buttons

    private void BrowseXml()
    {
        var file = FilePickerService.PickXmlFile();
        if (file is not null)
        {
            ViewWalkBy.XmlFile = file;
        }
    }

    private void BrowseNbIot()
    {
        var file = FilePickerService.PickNbIotFile();
        if (file is not null)
        {
            ViewNbIot.InputFile = file;
        }
    }

    private void BrowseSource()
    {
        var file = FilePickerService.PickTextFile();
        if (file is not null)
        {
            ViewSettings.SharedSourceFile = file;
            UpdateSourceViews();
        }
    }

    private void BrowseOutput()
    {
        var folder = FilePickerService.PickOutputFolder();
        if (folder is not null)
        {
            ViewWalkBy.OutputFolder = folder;
            ViewNbIot.OutputFolder = folder;
        }
    }

    private void BrowseZgrade()
    {
        foreach (var file in FilePickerService.PickZgradeFiles())
        {
            AddZgradeFile(file);
        }
    }

    private void AddZgradeFile(string file)
    {
        if (!_zgradeFiles.Contains(file))
        {
            _zgradeFiles.Add(file);
        }
    }

    #endregion

    #region Settings

    private void SaveSettings()
    {
        SaveSettingsFromUi();
        UpdateSourceViews();
        MessageBox.Show("Postavke su spremljene.", "DUKOM PLIN Utility", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Module actions

    private void RunWalkBy()
    {
        try
        {
            var source = ViewSettings.SharedSourceFile.Trim();

            if (!File.Exists(ViewWalkBy.XmlFile) || !File.Exists(source) || string.IsNullOrWhiteSpace(ViewWalkBy.OutputFolder))
            {
                MessageBox.Show("Provjeri XML, Source u Postavkama i izlaznu mapu.");
                return;
            }

            var result = WalkByService.Process(ViewWalkBy.XmlFile, source, ViewWalkBy.OutputFolder);
            ViewWalkBy.StatsText = $"XML: {result.XmlCount}   Matched: {result.Matched}   Missing: {result.Missing}";
            ViewLogs.LogText = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "WalkBy error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunNbIot()
    {
        try
        {
            if (!File.Exists(ViewNbIot.InputFile) || string.IsNullOrWhiteSpace(ViewNbIot.OutputFolder))
            {
                MessageBox.Show("Provjeri NB-IoT datoteku i izlaznu mapu.");
                return;
            }

            var result = NbIotService.Process(
                ViewNbIot.InputFile,
                ViewNbIot.OutputFolder,
                ViewNbIot.ExpectedDate,
                ViewNbIot.NormalizeCroatian);

            ViewNbIot.StatsText = $"Total: {result.Total}   Export: {result.Exported}   Missing UC: {result.MissingUserCode}   Date mismatch: {result.DateMismatch}";
            ViewLogs.LogText = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NB-IoT error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunZgrade()
    {
        try
        {
            var source = ViewSettings.SharedSourceFile.Trim();
            var output = FirstNonEmpty(
                ViewWalkBy.OutputFolder,
                ViewNbIot.OutputFolder,
                Path.GetDirectoryName(_zgradeFiles.FirstOrDefault() ?? string.Empty) ?? string.Empty);

            if (!File.Exists(source) || _zgradeFiles.Count == 0 || string.IsNullOrWhiteSpace(output))
            {
                MessageBox.Show("Provjeri Source u Postavkama, TXT fajlove i izlaznu mapu.");
                return;
            }

            var result = ZgradeService.Process(_zgradeFiles, source, output);
            ViewZgrade.IssuesSource = result.IssueList;
            ViewZgrade.StatsText = $"Rows: {result.Total}   Issues: {result.Issues}   Lower: {result.Lower}   Duplicates: {result.Duplicates}";
            ViewLogs.LogText = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Zgrade error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Open output / logs

    private void OpenOutputFolder()
    {
        var directory = FirstNonEmpty(ViewWalkBy.OutputFolder, ViewNbIot.OutputFolder);
        if (Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }
    }

    private void OpenLog()
    {
        var file = FilePickerService.PickLogFile();
        if (file is not null)
        {
            ViewLogs.LogText = LogViewerService.ReadSafe(file);
        }
    }

    #endregion
}
