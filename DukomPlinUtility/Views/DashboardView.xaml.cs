using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DukomPlinUtility.Views;

public partial class DashboardView : UserControl
{
    public event EventHandler? WalkByRequested;
    public event EventHandler? NbIotRequested;
    public event EventHandler? ZgradeRequested;

    public DashboardView() => InitializeComponent();

    public void SetSourceText(string source) => LblDashboardSource.Text = source;

    private void WalkByCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => WalkByRequested?.Invoke(this, EventArgs.Empty);
    private void NbIotCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => NbIotRequested?.Invoke(this, EventArgs.Empty);
    private void ZgradeCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ZgradeRequested?.Invoke(this, EventArgs.Empty);
}
