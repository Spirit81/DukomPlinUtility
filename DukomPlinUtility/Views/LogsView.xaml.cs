using System;
using System.Windows;
using System.Windows.Controls;

namespace DukomPlinUtility.Views;

public partial class LogsView : UserControl
{
    public event EventHandler? OpenLogRequested;

    public LogsView() => InitializeComponent();

    public string LogText { get => TxtLogViewer.Text; set => TxtLogViewer.Text = value; }

    private void OpenLog_Click(object sender, RoutedEventArgs e) => OpenLogRequested?.Invoke(this, EventArgs.Empty);
}
