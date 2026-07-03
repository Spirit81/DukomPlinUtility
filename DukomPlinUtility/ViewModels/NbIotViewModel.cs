using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
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

    public NbIotViewModel(LogsViewModel logs, DashboardViewModel dashboard, Action<string, string, double, bool> setStatus)
    {
        _logs = logs;
        _dashboard = dashboard;
        _setStatus = setStatus;
        BrowseInputCommand = new RelayCommand(BrowseInput);
        BrowseOutputCommand = new RelayCommand(BrowseOutput);
        RunCommand = new RelayCommand(Run);
        OpenOutputCommand = new RelayCommand(OpenOutputFolder);
    }

    public string InputFile { get => _inputFile; set => SetProperty(ref _inputFile, value); }
    public string OutputFolder { get => _outputFolder; set => SetProperty(ref _outputFolder, value); }
    public DateTime? ExpectedDate { get => _expectedDate; set => SetProperty(ref _expectedDate, value); }
    public bool NormalizeCroatian { get => _normalizeCroatian; set => SetProperty(ref _normalizeCroatian, value); }
    public string StatsText { get => _statsText; set => SetProperty(ref _statsText, value); }
    public ObservableCollection<ValidationItem> PreviewItems { get; } = new();

    public ICommand BrowseInputCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand OpenOutputCommand { get; }

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
            var result = NbIotService.Process(InputFile, OutputFolder, ExpectedDate, NormalizeCroatian);
            StatsText = $"Total: {result.Total}   Export: {result.Exported}   Missing UC: {result.MissingUserCode}   Date mismatch: {result.DateMismatch}";
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
    }

    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
        {
            Process.Start(new ProcessStartInfo { FileName = OutputFolder, UseShellExecute = true });
        }
    }
}
