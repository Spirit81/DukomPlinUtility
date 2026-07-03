using System.Windows;
using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Models;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly Window _window;
    private readonly AppSettings _settings;
    private ViewModelBase _currentViewModel;
    private AppModule _currentModule;
    private string _statusText = "Spreman";
    private string _statusDetails = "DUKOM PLIN Utility Professional";
    private double _progressValue;
    private bool _isBusy;

    public MainViewModel(Window window)
    {
        _window = window;
        _settings = SettingsService.Load();
        WindowSettingsService.Apply(_window, _settings);

        Logs = new LogsViewModel();
        Dashboard = new DashboardViewModel(NavigateTo);
        Settings = new SettingsViewModel(SaveSettingsWithMessage, UpdateSharedSourceViews);
        WalkBy = new WalkByViewModel(() => Settings.SharedSourceFile, Logs, Dashboard, SetOperationStatus, NavigateTo);
        NbIot = new NbIotViewModel(Logs, Dashboard, SetOperationStatus);
        Zgrade = new ZgradeViewModel(() => Settings.SharedSourceFile, () => FirstNonEmpty(WalkBy.OutputFolder, NbIot.OutputFolder, _settings.LastOutputFolder), Logs, Dashboard, SetOperationStatus);
        About = new AboutViewModel();

        NavigateCommand = new RelayCommand(parameter => NavigateTo(AppModuleExtensions.FromString(parameter?.ToString())));

        LoadSettingsToViewModels();
        UpdateSharedSourceViews();

        _currentViewModel = Dashboard;
        NavigateTo(AppModuleExtensions.FromString(_settings.LastModule));
    }

    public DashboardViewModel Dashboard { get; }
    public WalkByViewModel WalkBy { get; }
    public NbIotViewModel NbIot { get; }
    public ZgradeViewModel Zgrade { get; }
    public LogsViewModel Logs { get; }
    public SettingsViewModel Settings { get; }
    public AboutViewModel About { get; }

    public ICommand NavigateCommand { get; }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public AppModule CurrentModule
    {
        get => _currentModule;
        private set => SetProperty(ref _currentModule, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string StatusDetails
    {
        get => _statusDetails;
        set => SetProperty(ref _statusDetails, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private void LoadSettingsToViewModels()
    {
        Settings.SharedSourceFile = _settings.SharedSourceFile;
        WalkBy.OutputFolder = _settings.LastOutputFolder;
        NbIot.OutputFolder = _settings.LastOutputFolder;
        NbIot.ExpectedDate = DateTime.Today;
    }

    private void NavigateTo(AppModule module)
    {
        CurrentModule = module;
        CurrentViewModel = module switch
        {
            AppModule.WalkBy => WalkBy,
            AppModule.NbIot => NbIot,
            AppModule.Zgrade => Zgrade,
            AppModule.Logs => Logs,
            AppModule.Settings => Settings,
            AppModule.About => About,
            _ => Dashboard
        };

        _settings.LastModule = module.ToString();
        StatusText = module == AppModule.Dashboard ? "Spreman" : $"Otvoren modul: {module}";
        StatusDetails = Dashboard.SourceStatus;
        UpdateSharedSourceViews();
    }

    private void UpdateSharedSourceViews()
    {
        var source = string.IsNullOrWhiteSpace(Settings.SharedSourceFile)
            ? "Source nije postavljen"
            : Settings.SharedSourceFile;

        Dashboard.SourceText = source;
        WalkBy.SourceText = source;
        StatusDetails = Dashboard.SourceStatus;
    }

    private void SaveSettingsWithMessage()
    {
        SaveSettings();
        UpdateSharedSourceViews();
        SetOperationStatus("Postavke spremljene", "Shared Source i postavke su spremljene.", 100, false);
    }

    public void SaveSettings()
    {
        try
        {
            WindowSettingsService.Capture(_window, _settings);
            _settings.SharedSourceFile = Settings.SharedSourceFile.Trim();
            _settings.LastOutputFolder = FirstNonEmpty(WalkBy.OutputFolder, NbIot.OutputFolder, _settings.LastOutputFolder);
            SettingsService.Save(_settings);
        }
        catch
        {
            // Settings must never crash the application.
        }
    }

    private void SetOperationStatus(string text, string details, double progress, bool isBusy)
    {
        StatusText = text;
        StatusDetails = details;
        ProgressValue = Math.Max(0, Math.Min(100, progress));
        IsBusy = isBusy;
    }

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
