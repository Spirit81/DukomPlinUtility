using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class NbIotViewModel : ViewModelBase
{
    private readonly LogsViewModel _logs;
    private string _inputFile = string.Empty;
    private string _outputFolder = string.Empty;
    private DateTime? _expectedDate = DateTime.Today;
    private bool _normalizeCroatian = true;
    private string _statsText = string.Empty;

    public NbIotViewModel(LogsViewModel logs)
    {
        _logs = logs;
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

            var result = NbIotService.Process(InputFile, OutputFolder, ExpectedDate, NormalizeCroatian);
            StatsText = $"Total: {result.Total}   Export: {result.Exported}   Missing UC: {result.MissingUserCode}   Date mismatch: {result.DateMismatch}";
            _logs.LogText = LogViewerService.ReadSafe(result.LogPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NB-IoT error", MessageBoxButton.OK, MessageBoxImage.Error);
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
