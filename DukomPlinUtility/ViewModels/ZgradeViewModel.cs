using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Models;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class ZgradeViewModel : ViewModelBase
{
    private readonly Func<string> _getSharedSource;
    private readonly Func<string> _getDefaultOutputFolder;
    private readonly LogsViewModel _logs;
    private readonly DashboardViewModel _dashboard;
    private readonly Action<string, string, double, bool> _setStatus;
    private string _statsText = string.Empty;

    public ZgradeViewModel(Func<string> getSharedSource, Func<string> getDefaultOutputFolder, LogsViewModel logs, DashboardViewModel dashboard, Action<string, string, double, bool> setStatus)
    {
        _getSharedSource = getSharedSource;
        _getDefaultOutputFolder = getDefaultOutputFolder;
        _logs = logs;
        _dashboard = dashboard;
        _setStatus = setStatus;
        BrowseFilesCommand = new RelayCommand(BrowseFiles);
        ClearCommand = new RelayCommand(() => Files.Clear());
        RunCommand = new RelayCommand(Run);
        OpenOutputCommand = new RelayCommand(OpenOutputFolder);
    }

    public ObservableCollection<string> Files { get; } = new();
    public ObservableCollection<ZgradeIssue> Issues { get; } = new();

    public string StatsText
    {
        get => _statsText;
        set => SetProperty(ref _statsText, value);
    }

    public ICommand BrowseFilesCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand OpenOutputCommand { get; }

    public void AddFile(string file)
    {
        if (!Files.Contains(file)) Files.Add(file);
    }

    private void BrowseFiles()
    {
        foreach (var file in FilePickerService.PickZgradeFiles())
        {
            AddFile(file);
        }
    }

    private void Run()
    {
        try
        {
            var source = _getSharedSource().Trim();
            var output = ResolveOutputFolder();

            if (!File.Exists(source) || Files.Count == 0 || string.IsNullOrWhiteSpace(output))
            {
                MessageBox.Show("Provjeri Source u Postavkama, TXT fajlove i izlaznu mapu.");
                return;
            }

            _setStatus("Zgrade obrada u tijeku", $"TXT fajlova: {Files.Count}", 15, true);
            var result = ZgradeService.Process(Files, source, output);
            Issues.Clear();
            foreach (var issue in result.IssueList)
            {
                Issues.Add(issue);
            }

            StatsText = $"Rows: {result.Total}   Issues: {result.Issues}   Lower: {result.Lower}   Duplicates: {result.Duplicates}";
            _logs.LogText = LogViewerService.ReadSafe(result.LogPath);
            _dashboard.UpdateZgrade(result.Total, result.Issues, result.Lower, result.Duplicates);
            _setStatus("Zgrade završeno", $"Issues: {result.Issues}, Duplicates: {result.Duplicates}", 100, false);
        }
        catch (Exception ex)
        {
            _setStatus("Zgrade greška", ex.Message, 0, false);
            MessageBox.Show(ex.Message, "Zgrade error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string ResolveOutputFolder()
    {
        var configured = _getDefaultOutputFolder();
        if (!string.IsNullOrWhiteSpace(configured)) return configured;
        return Path.GetDirectoryName(Files.FirstOrDefault() ?? string.Empty) ?? string.Empty;
    }

    private void OpenOutputFolder()
    {
        var directory = ResolveOutputFolder();
        if (Directory.Exists(directory))
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }
    }
}
