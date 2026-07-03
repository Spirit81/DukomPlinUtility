using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class LogsViewModel : ViewModelBase
{
    private string _logText = string.Empty;

    public LogsViewModel()
    {
        OpenLogCommand = new RelayCommand(OpenLog);
    }

    public string LogText
    {
        get => _logText;
        set => SetProperty(ref _logText, value);
    }

    public ICommand OpenLogCommand { get; }

    private void OpenLog()
    {
        var file = FilePickerService.PickLogFile();
        if (file is not null)
        {
            LogText = LogViewerService.ReadSafe(file);
        }
    }
}
