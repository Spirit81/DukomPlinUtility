using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DukomPlinUtility.Helpers;
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

        LoadSettingsToUi();
        ListZgradeFiles.ItemsSource = _zgradeFiles;

        UpdateSourceViews();
        ShowView(AppModuleExtensions.FromString(_settings.LastModule));

        Closing += (_, _) => SaveSettingsFromUi();
    }

    private void LoadSettingsToUi()
    {
        TxtSharedSource.Text = _settings.SharedSourceFile;
        TxtOutput.Text = _settings.LastOutputFolder;
        TxtNbOutput.Text = _settings.LastOutputFolder;
        DpExpectedDate.SelectedDate = DateTime.Today;
    }

    private void SaveSettingsFromUi()
    {
        try
        {
            WindowSettingsService.Capture(this, _settings);
            _settings.SharedSourceFile = TxtSharedSource.Text.Trim();
            _settings.LastOutputFolder = UiHelper.FirstNonEmpty(TxtOutput.Text, TxtNbOutput.Text, _settings.LastOutputFolder);
            SettingsService.Save(_settings);
        }
        catch
        {
            // Settings must never crash the application.
        }
    }

    // FirstNonEmpty moved to Helpers.UiHelper

    #region Navigation

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag })
        {
            ShowView(AppModuleExtensions.FromString(tag));
        }
    }

    private void OpenWalkByCard(object sender, MouseButtonEventArgs e) => ShowView(AppModule.WalkBy);
    private void OpenNbIotCard(object sender, MouseButtonEventArgs e) => ShowView(AppModule.NbIot);
    private void OpenZgradeCard(object sender, MouseButtonEventArgs e) => ShowView(AppModule.Zgrade);
    private void OpenSettings_Click(object sender, RoutedEventArgs e) => ShowView(AppModule.Settings);

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
        foreach (var view in new[]
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
        var source = string.IsNullOrWhiteSpace(TxtSharedSource.Text)
            ? "Source nije postavljen"
            : TxtSharedSource.Text;

        LblDashboardSource.Text = source;
        TxtWalkBySourceView.Text = source;
    }

    #endregion

    #region Drag and drop

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);

    private void Xml_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFileByExtension(e, ".xml");
        if (file is not null)
        {
            TxtXml.Text = file;
        }
    }

    private void NbIot_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFile(e);
        if (file is not null)
        {
            TxtNbIot.Text = file;
        }
    }

    private void Output_Drop(object sender, DragEventArgs e)
    {
        var folder = DragDropHelper.DroppedDirectoryOrParent(e);
        if (folder is not null)
        {
            TxtOutput.Text = folder;
            TxtNbOutput.Text = folder;
        }
    }

    private void Zgrade_Drop(object sender, DragEventArgs e)
    {
        foreach (var file in DragDropHelper.GetDroppedFiles(e).Where(File.Exists))
        {
            _zgradeFiles.AddIfNotExists(file);
        }
    }

    #endregion

    #region Browse buttons

    private void BrowseXml_Click(object sender, RoutedEventArgs e)
    {
        var file = FilePickerService.PickXmlFile();
        if (file is not null)
        {
            TxtXml.Text = file;
        }
    }

    private void BrowseNbIot_Click(object sender, RoutedEventArgs e)
    {
        var file = FilePickerService.PickNbIotFile();
        if (file is not null)
        {
            TxtNbIot.Text = file;
        }
    }

    private void BrowseSource_Click(object sender, RoutedEventArgs e)
    {
        var file = FilePickerService.PickTextFile();
        if (file is not null)
        {
            TxtSharedSource.Text = file;
            UpdateSourceViews();
        }
    }

    private void BrowseOutput_Click(object sender, RoutedEventArgs e)
    {
        var folder = FilePickerService.PickOutputFolder();
        if (folder is not null)
        {
            TxtOutput.Text = folder;
            TxtNbOutput.Text = folder;
        }
    }

    private void BrowseZgrade_Click(object sender, RoutedEventArgs e)
    {
        foreach (var file in FilePickerService.PickZgradeFiles())
        {
            _zgradeFiles.AddIfNotExists(file);
        }
    }

    private void ClearZgrade_Click(object sender, RoutedEventArgs e) => _zgradeFiles.Clear();

    #endregion

    #region Settings

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        SaveSettingsFromUi();
        UpdateSourceViews();
        MessageBox.Show("Postavke su spremljene.", "DUKOM PLIN Utility", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Module actions

    private void RunWalkBy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var source = TxtSharedSource.Text.Trim();

            if (!File.Exists(TxtXml.Text) || !File.Exists(source) || string.IsNullOrWhiteSpace(TxtOutput.Text))
            {
                MessageBox.Show("Provjeri XML, Source u Postavkama i izlaznu mapu.");
                return;
            }

            var result = WalkByService.Process(TxtXml.Text, source, TxtOutput.Text);
            LblWalkByStats.Text = $"XML: {result.XmlCount}   Matched: {result.Matched}   Missing: {result.Missing}";
            TxtLogViewer.Text = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "WalkBy error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunNbIot_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(TxtNbIot.Text) || string.IsNullOrWhiteSpace(TxtNbOutput.Text))
            {
                MessageBox.Show("Provjeri NB-IoT datoteku i izlaznu mapu.");
                return;
            }

            var result = NbIotService.Process(
                TxtNbIot.Text,
                TxtNbOutput.Text,
                DpExpectedDate.SelectedDate,
                ChkCroatian.IsChecked == true);

            LblNbStats.Text = $"Total: {result.Total}   Export: {result.Exported}   Missing UC: {result.MissingUserCode}   Date mismatch: {result.DateMismatch}";
            TxtLogViewer.Text = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NB-IoT error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunZgrade_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var source = TxtSharedSource.Text.Trim();
            var output = UiHelper.FirstNonEmpty(TxtOutput.Text, TxtNbOutput.Text, Path.GetDirectoryName(_zgradeFiles.FirstOrDefault() ?? string.Empty) ?? string.Empty);

            if (!File.Exists(source) || _zgradeFiles.Count == 0 || string.IsNullOrWhiteSpace(output))
            {
                MessageBox.Show("Provjeri Source u Postavkama, TXT fajlove i izlaznu mapu.");
                return;
            }

            var result = ZgradeService.Process(_zgradeFiles, source, output);
            GridZgradeIssues.ItemsSource = result.IssueList;
            LblZgradeStats.Text = $"Rows: {result.Total}   Issues: {result.Issues}   Lower: {result.Lower}   Duplicates: {result.Duplicates}";
            TxtLogViewer.Text = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Zgrade error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Open output / logs

    private void OpenOutput_Click(object sender, RoutedEventArgs e)
    {
        var directory = UiHelper.FirstNonEmpty(TxtOutput.Text, TxtNbOutput.Text);
        SystemService.OpenFolder(directory);
    }

    private void OpenLog_Click(object sender, RoutedEventArgs e)
    {
        var file = FilePickerService.PickLogFile();
        if (file is not null)
        {
            TxtLogViewer.Text = LogViewerService.ReadSafe(file);
        }
    }

    #endregion
}
