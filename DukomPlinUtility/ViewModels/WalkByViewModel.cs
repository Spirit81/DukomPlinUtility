using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Models;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class WalkByViewModel : ViewModelBase
{
    private readonly Func<string> _getSharedSource;
    private readonly LogsViewModel _logs;
    private readonly DashboardViewModel _dashboard;
    private readonly Action<string, string, double, bool> _setStatus;
    private readonly Action<AppModule> _navigate;
    private string _xmlFile = string.Empty;
    private string _outputFolder = string.Empty;
    private string _sourceText = "Source nije postavljen";
    private string _statsText = string.Empty;
    private string _xmlCountText = "-";
    private string _matchedText = "-";
    private string _missingText = "-";
    private string _successText = "-";
    private string _durationText = "-";
    private string _lastRunText = "-";
    private string _previewTotalText = "Ukupno: 0";
    private string _searchText = string.Empty;
    private string _activeFilter = "All";

    public WalkByViewModel(Func<string> getSharedSource, LogsViewModel logs, DashboardViewModel dashboard, Action<string, string, double, bool> setStatus, Action<AppModule> navigate)
    {
        _getSharedSource = getSharedSource;
        _logs = logs;
        _dashboard = dashboard;
        _setStatus = setStatus;
        _navigate = navigate;
        BrowseXmlCommand = new RelayCommand(BrowseXml);
        BrowseOutputCommand = new RelayCommand(BrowseOutput);
        OpenSettingsCommand = new RelayCommand(() => _navigate(AppModule.Settings));
        RunCommand = new RelayCommand(Run);
        OpenOutputCommand = new RelayCommand(OpenOutputFolder);
        ShowAllCommand = new RelayCommand(() => SetFilter("All"));
        ShowOkCommand = new RelayCommand(() => SetFilter("OK"));
        ShowWarningCommand = new RelayCommand(() => SetFilter("Warning"));
        ShowErrorCommand = new RelayCommand(() => SetFilter("Error"));
        PreviewView = CollectionViewSource.GetDefaultView(PreviewItems);
        PreviewView.Filter = FilterPreviewItem;
    }

    public string XmlFile { get => _xmlFile; set => SetProperty(ref _xmlFile, value); }
    public string OutputFolder { get => _outputFolder; set => SetProperty(ref _outputFolder, value); }
    public string SourceText { get => _sourceText; set => SetProperty(ref _sourceText, value); }
    public string StatsText { get => _statsText; set => SetProperty(ref _statsText, value); }
    public string XmlCountText { get => _xmlCountText; set => SetProperty(ref _xmlCountText, value); }
    public string MatchedText { get => _matchedText; set => SetProperty(ref _matchedText, value); }
    public string MissingText { get => _missingText; set => SetProperty(ref _missingText, value); }
    public string SuccessText { get => _successText; set => SetProperty(ref _successText, value); }
    public string DurationText { get => _durationText; set => SetProperty(ref _durationText, value); }
    public string LastRunText { get => _lastRunText; set => SetProperty(ref _lastRunText, value); }
    public string PreviewTotalText { get => _previewTotalText; set => SetProperty(ref _previewTotalText, value); }
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshPreview();
            }
        }
    }
    public string ActiveFilter { get => _activeFilter; set => SetProperty(ref _activeFilter, value); }
    public ObservableCollection<ValidationItem> PreviewItems { get; } = new();
    public ICollectionView PreviewView { get; }

    public ICommand BrowseXmlCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand OpenOutputCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand ShowOkCommand { get; }
    public ICommand ShowWarningCommand { get; }
    public ICommand ShowErrorCommand { get; }

    private void BrowseXml()
    {
        var file = FilePickerService.PickXmlFile();
        if (file is not null) XmlFile = file;
    }

    private void BrowseOutput()
    {
        var folder = FilePickerService.PickOutputFolder();
        if (folder is not null) OutputFolder = folder;
    }

    public void SetDroppedXml(string file) => XmlFile = file;
    public void SetDroppedOutput(string folder) => OutputFolder = folder;

    private void Run()
    {
        try
        {
            var source = _getSharedSource().Trim();
            if (!File.Exists(XmlFile) || !File.Exists(source) || string.IsNullOrWhiteSpace(OutputFolder))
            {
                MessageBox.Show("Provjeri XML, Source u Postavkama i izlaznu mapu.");
                return;
            }

            _setStatus("WalkBy obrada u tijeku", Path.GetFileName(XmlFile), 15, true);
            var sw = Stopwatch.StartNew();
            var result = WalkByService.Process(XmlFile, source, OutputFolder);
            sw.Stop();
            StatsText = $"XML: {result.XmlCount}   Matched: {result.Matched}   Missing: {result.Missing}";
            XmlCountText = result.XmlCount.ToString("N0");
            MatchedText = result.Matched.ToString("N0");
            MissingText = result.Missing.ToString("N0");
            SuccessText = result.XmlCount > 0 ? $"{(result.Matched * 100.0 / result.XmlCount):N2} %" : "-";
            DurationText = sw.Elapsed.ToString(@"hh\:mm\:ss");
            LastRunText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            _logs.LogText = LogViewerService.ReadSafe(result.LogPath);
            LoadPreview(result.Preview);
            _dashboard.UpdateWalkBy(result.XmlCount, result.Matched, result.Missing);
            _setStatus("WalkBy završeno", $"Matched: {result.Matched}, Missing: {result.Missing}", 100, false);
        }
        catch (Exception ex)
        {
            _setStatus("WalkBy greška", ex.Message, 0, false);
            MessageBox.Show(ex.Message, "WalkBy error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadPreview(IEnumerable<ValidationItem> items)
    {
        PreviewItems.Clear();
        foreach (var item in items)
        {
            PreviewItems.Add(item);
        }
        RefreshPreview();
    }

    private void SetFilter(string filter)
    {
        ActiveFilter = filter;
        RefreshPreview();
    }

    private bool FilterPreviewItem(object item)
    {
        if (item is not ValidationItem validationItem) return false;

        if (!string.Equals(ActiveFilter, "All", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(validationItem.StatusLevel, ActiveFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var query = SearchText.Trim();
        return Contains(validationItem.UserCode, query) ||
               Contains(validationItem.Name, query) ||
               Contains(validationItem.Meter, query) ||
               Contains(validationItem.Reading, query) ||
               Contains(validationItem.PreviousReading, query) ||
               Contains(validationItem.Date, query) ||
               Contains(validationItem.Status, query) ||
               Contains(validationItem.Message, query);
    }

    private void RefreshPreview()
    {
        PreviewView.Refresh();
        PreviewTotalText = $"Ukupno: {PreviewView.Cast<object>().Count():N0}";
    }

    private static bool Contains(string? value, string query)
        => !string.IsNullOrEmpty(value) && value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
        {
            Process.Start(new ProcessStartInfo { FileName = OutputFolder, UseShellExecute = true });
        }
    }
}
