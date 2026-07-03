using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
    }

    public string XmlFile { get => _xmlFile; set => SetProperty(ref _xmlFile, value); }
    public string OutputFolder { get => _outputFolder; set => SetProperty(ref _outputFolder, value); }
    public string SourceText { get => _sourceText; set => SetProperty(ref _sourceText, value); }
    public string StatsText { get => _statsText; set => SetProperty(ref _statsText, value); }
    public ObservableCollection<ValidationItem> PreviewItems { get; } = new();

    public ICommand BrowseXmlCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand OpenOutputCommand { get; }

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
            var result = WalkByService.Process(XmlFile, source, OutputFolder);
            StatsText = $"XML: {result.XmlCount}   Matched: {result.Matched}   Missing: {result.Missing}";
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
    }

    private void OpenOutputFolder()
    {
        if (Directory.Exists(OutputFolder))
        {
            Process.Start(new ProcessStartInfo { FileName = OutputFolder, UseShellExecute = true });
        }
    }
}
