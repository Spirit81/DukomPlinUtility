using System.Windows;
using DukomPlinUtility.ViewModels;

namespace DukomPlinUtility;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel(this);
        DataContext = _viewModel;
        Closing += (_, _) => _viewModel.SaveSettings();
    }
}
