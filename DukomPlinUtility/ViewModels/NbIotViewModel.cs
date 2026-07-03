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

public sealed class NbIotViewModel : ViewModelBase
{
    private readonly LogsViewModel _logs;
    private readonly DashboardViewModel _dashboard;
    private readonly Action<string, string, double, bool> _setStatus;
    private string _inputFile = string.Empty;
    private string _outputFolder = string.Empty;
    private DateTime? _expectedDate = DateTime.Today;
    private bool _normalizeCroatian = true;
    private string _statsText = string.Empty;
    private string _totalText = "-";
    private string _exportedText = "-";
    private string _missingUserCodeText = "-";
    private string _dateMismatchText = "-";
    private string _durationText = "-";
    private string _lastRunText = "-";
    private string _previewTotalText = "Ukupno: 0";
    private string _searchText = string.Empty;
    private string _activeFilter = "All";

    public NbIotViewModel(LogsViewModel logs, DashboardViewModel dashboard, Action<string, string, double, bool> setStatus)
    {
        _logs = logs;
        _dashboard = dashboard;
        _setStatus = setStatus;
        BrowseInputCommand = new RelayCommand(BrowseInput);
        BrowseOutputCommand = new RelayCommand(BrowseOutput);
        RunCommand = new RelayCommand(Run);
        OpenOutputCommand = new RelayCommand(OpenOutputFolder);
        ShowAllCommand = new RelayCommand(() => SetFilter("All"));
        ShowOkCommand = new RelayCommand(() => SetFilter("OK"));
        ShowWarningCommand = new RelayCommand(() => SetFilter("Warning"));
        ShowErrorCommand = new RelayCommand(() => SetFilter("Error"));
        PreviewView = CollectionViewSource.GetDefaultView(PreviewItems);
        PreviewView.Filter = FilterPreviewItem;
    }

    public string InputFile { get => _inputFile; set => SetProperty(ref _inputFile, value); }
    public string OutputFolder { get => _outputFolder; set => SetProperty(ref _outputFolder, value); }
    public DateTime? ExpectedDate { get => _expectedDate; set => SetProperty(ref _expectedDate, value); }
    public bool NormalizeCroatian { get => _normalizeCroatian; set => SetProperty(ref _normalizeCroatian, value); }
    public string StatsText { get => _statsText; set => SetProperty(ref _statsText, value); }
    public string TotalText { get => _totalText; set => SetProperty(ref _totalText, value); }
    public string ExportedText { get => _exportedText; set => SetProperty(ref _exportedText, value); }
    public string MissingUserCodeText { get => _missingUserCodeText; set => SetProperty(ref _missingUserCodeText, value); }
    public string DateMismatchText { get => _dateMismatchText; set => SetProperty(ref _dateMismatchText, value); }
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

    public ICommand BrowseInputCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand OpenOutputCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand ShowOkCommand { get; }
    public ICommand ShowWarningCommand { get; }
    public ICommand ShowErrorCommand { get; }

    private void BrowseInput()
    {
        var file = FilePickerService.PickNbIotFile();
        if (file is not null) InputFile = file;
    }

    private void BrowseOutput()
    {
        var folder = FilePickerService.PickOutputFolder();
        if (folder is not null) OutputFolder = folder;
    }

    public void SetDroppedInput(string file) => InputFile = file;
    public void SetDroppedOutput(string folder) => OutputFolder = folder;

    private void Run()
    {
        try
        {
            if (!File.Exists(InputFile) || string.IsNullOrWhiteSpace(OutputFolder))
            {
                MessageBox.Show("Provjeri NB-IoT datoteku i izlaznu mapu.");
                return;
            }

            _setStatus("NB-IoT obrada u tijeku", Path.GetFileName(InputFile), 15, true);
            var sw = Stopwatch.StartNew();
            var result = NbIotService.Process(InputFile, OutputFolder, ExpectedDate, NormalizeCroatian);
            sw.Stop();
            StatsText = $"Total: {result.Total}   Export: {result.Exported}   Missing UC: {result.MissingUserCode}   Date mismatch: {result.DateMismatch}";
            TotalText = result.Total.ToString("N0");
            ExportedText = result.Exported.ToString("N0");
            MissingUserCodeText = result.MissingUserCode.ToString("N0");
            DateMismatchText = result.DateMismatch.ToString("N0");
            DurationText = sw.Elapsed.ToString(@"hh\:mm\:ss");
            LastRunText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            _logs.LogText = LogViewerService.ReadSafe(result.LogPath);
            LoadPreview(result.Preview);
            _dashboard.UpdateNbIot(result.Total, result.Exported, result.MissingUserCode, result.DateMismatch);
            _setStatus("NB-IoT završeno", $"Export: {result.Exported}, Warnings: {result.MissingUserCode + result.DateMismatch}", 100, false);
        }
        catch (Exception ex)
        {
            _setStatus("NB-IoT greška", ex.Message, 0, false);
            MessageBox.Show(ex.Message, "NB-IoT error", MessageBoxButton.OK, MessageBoxImage.Error);
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
