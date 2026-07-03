using System.Windows;
using System.Windows.Input;
using DukomPlinUtility.Infrastructure;
using DukomPlinUtility.Services;

namespace DukomPlinUtility.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private string _sharedSourceFile = string.Empty;
    private readonly Action _saveRequested;
    private readonly Action _sourceChanged;

    public SettingsViewModel(Action saveRequested, Action sourceChanged)
    {
        _saveRequested = saveRequested;
        _sourceChanged = sourceChanged;
        BrowseSourceCommand = new RelayCommand(BrowseSource);
        SaveCommand = new RelayCommand(Save);
    }

    public string SharedSourceFile
    {
        get => _sharedSourceFile;
        set
        {
            if (SetProperty(ref _sharedSourceFile, value))
            {
                _sourceChanged();
            }
        }
    }

    public ICommand BrowseSourceCommand { get; }
    public ICommand SaveCommand { get; }

    private void BrowseSource()
    {
        var file = FilePickerService.PickTextFile();
        if (file is not null)
        {
            SharedSourceFile = file;
        }
    }

    private void Save()
    {
        _saveRequested();
        MessageBox.Show("Postavke su spremljene.", "DUKOM PLIN Utility", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
