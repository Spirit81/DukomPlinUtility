using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private string _sourceText = "Source nije postavljen";

    public DashboardViewModel(Action<AppModule> navigate)
    {
        OpenWalkByCommand = new RelayCommand(() => navigate(AppModule.WalkBy));
        OpenNbIotCommand = new RelayCommand(() => navigate(AppModule.NbIot));
        OpenZgradeCommand = new RelayCommand(() => navigate(AppModule.Zgrade));
        OpenSettingsCommand = new RelayCommand(() => navigate(AppModule.Settings));
    }

    public string SourceText
    {
        get => _sourceText;
        set => SetProperty(ref _sourceText, value);
    }

    public ICommand OpenWalkByCommand { get; }
    public ICommand OpenNbIotCommand { get; }
    public ICommand OpenZgradeCommand { get; }
    public ICommand OpenSettingsCommand { get; }
}
