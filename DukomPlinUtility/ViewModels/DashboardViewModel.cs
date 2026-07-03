using System.IO;
using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private string _sourceText = "Source nije postavljen";
    private string _sourceStatus = "⚠ Source nije postavljen";
    private string _sourceDetails = "Odaberi ZaRtf.txt u Postavkama.";
    private string _sourceStatusLevel = "Warning";

    public DashboardViewModel(Action<AppModule> navigate)
    {
        OpenWalkByCommand = new RelayCommand(() => navigate(AppModule.WalkBy));
        OpenNbIotCommand = new RelayCommand(() => navigate(AppModule.NbIot));
        OpenZgradeCommand = new RelayCommand(() => navigate(AppModule.Zgrade));
        OpenSettingsCommand = new RelayCommand(() => navigate(AppModule.Settings));
        OpenLogsCommand = new RelayCommand(() => navigate(AppModule.Logs));

        WalkBySummary.Module = "WalkBy";
        NbIotSummary.Module = "NB-IoT";
        ZgradeSummary.Module = "Zgrade";
    }

    public string SourceText
    {
        get => _sourceText;
        set
        {
            if (SetProperty(ref _sourceText, value))
            {
                RefreshSourceStatus();
            }
        }
    }

    public string SourceStatus { get => _sourceStatus; set => SetProperty(ref _sourceStatus, value); }
    public string SourceDetails { get => _sourceDetails; set => SetProperty(ref _sourceDetails, value); }
    public string SourceStatusLevel { get => _sourceStatusLevel; set => SetProperty(ref _sourceStatusLevel, value); }

    public OperationSummary WalkBySummary { get; } = new();
    public OperationSummary NbIotSummary { get; } = new();
    public OperationSummary ZgradeSummary { get; } = new();

    public ICommand OpenWalkByCommand { get; }
    public ICommand OpenNbIotCommand { get; }
    public ICommand OpenZgradeCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenLogsCommand { get; }

    public void UpdateWalkBy(int xmlCount, int matched, int missing)
    {
        WalkBySummary.LastRun = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        WalkBySummary.MainStat = $"XML: {xmlCount}";
        WalkBySummary.SecondaryStat = $"Matched: {matched}";
        WalkBySummary.WarningStat = $"Missing: {missing}";
        WalkBySummary.Status = missing > 0 ? "Warning" : "OK";
        WalkBySummary.StatusLevel = missing > 0 ? "Warning" : "OK";
        NotifySummaryChanged();
    }

    public void UpdateNbIot(int total, int exported, int missingUserCode, int dateMismatch)
    {
        NbIotSummary.LastRun = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        NbIotSummary.MainStat = $"Total: {total}";
        NbIotSummary.SecondaryStat = $"Export: {exported}";
        NbIotSummary.WarningStat = $"Missing UC: {missingUserCode}   Date: {dateMismatch}";
        NbIotSummary.Status = (missingUserCode + dateMismatch) > 0 ? "Warning" : "OK";
        NbIotSummary.StatusLevel = (missingUserCode + dateMismatch) > 0 ? "Warning" : "OK";
        NotifySummaryChanged();
    }

    public void UpdateZgrade(int rows, int issues, int lower, int duplicates)
    {
        ZgradeSummary.LastRun = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        ZgradeSummary.MainStat = $"Rows: {rows}";
        ZgradeSummary.SecondaryStat = $"Issues: {issues}";
        ZgradeSummary.WarningStat = $"Lower: {lower}   Duplicates: {duplicates}";
        ZgradeSummary.Status = issues > 0 ? "Warning" : "OK";
        ZgradeSummary.StatusLevel = issues > 0 ? "Warning" : "OK";
        NotifySummaryChanged();
    }

    private void RefreshSourceStatus()
    {
        if (File.Exists(SourceText))
        {
            var info = new FileInfo(SourceText);
            SourceStatus = "✔ Shared Source OK";
            SourceDetails = $"{info.Name} · Updated: {info.LastWriteTime:dd.MM.yyyy HH:mm}";
            SourceStatusLevel = "OK";
        }
        else
        {
            SourceStatus = "⚠ Source nije postavljen";
            SourceDetails = "Odaberi ZaRtf.txt u Postavkama.";
            SourceStatusLevel = "Warning";
        }
    }

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(WalkBySummary));
        OnPropertyChanged(nameof(NbIotSummary));
        OnPropertyChanged(nameof(ZgradeSummary));
    }
}
