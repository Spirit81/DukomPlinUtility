using System;
using System.Windows;
using System.Windows.Controls;

namespace DukomPlinUtility.Views;

public partial class SettingsView : UserControl
{
    public event EventHandler? BrowseSourceRequested;
    public event EventHandler? SaveRequested;

    public SettingsView() => InitializeComponent();

    public string SharedSourceFile { get => TxtSharedSource.Text; set => TxtSharedSource.Text = value; }

    private void BrowseSource_Click(object sender, RoutedEventArgs e) => BrowseSourceRequested?.Invoke(this, EventArgs.Empty);
    private void SaveSettings_Click(object sender, RoutedEventArgs e) => SaveRequested?.Invoke(this, EventArgs.Empty);
}
