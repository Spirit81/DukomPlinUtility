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

    public MainViewModel(Window window)
    {
        _window = window;
        _settings = SettingsService.Load();
        WindowSettingsService.Apply(_window, _settings);

        Logs = new LogsViewModel();
        Dashboard = new DashboardViewModel(NavigateTo);
        Settings = new SettingsViewModel(SaveSettingsWithMessage, UpdateSharedSourceViews);
        WalkBy = new WalkByViewModel(() => Settings.SharedSourceFile, Logs, NavigateTo);
        NbIot = new NbIotViewModel(Logs);
        Zgrade = new ZgradeViewModel(() => Settings.SharedSourceFile, () => FirstNonEmpty(WalkBy.OutputFolder, NbIot.OutputFolder, _settings.LastOutputFolder), Logs);
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
        UpdateSharedSourceViews();
    }

    private void UpdateSharedSourceViews()
    {
        var source = string.IsNullOrWhiteSpace(Settings.SharedSourceFile)
            ? "Source nije postavljen"
            : Settings.SharedSourceFile;

        Dashboard.SourceText = source;
        WalkBy.SourceText = source;
    }

    private void SaveSettingsWithMessage()
    {
        SaveSettings();
        UpdateSharedSourceViews();
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

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
