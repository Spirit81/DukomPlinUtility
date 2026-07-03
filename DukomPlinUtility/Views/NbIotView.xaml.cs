using System;
using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;

namespace DukomPlinUtility.Views;

public partial class NbIotView : UserControl
{
    public event EventHandler? BrowseNbIotRequested;
    public event EventHandler? RunRequested;
    public event EventHandler? OpenOutputRequested;

    public NbIotView() => InitializeComponent();

    public string InputFile { get => TxtNbIot.Text; set => TxtNbIot.Text = value; }
    public string OutputFolder { get => TxtNbOutput.Text; set => TxtNbOutput.Text = value; }
    public DateTime? ExpectedDate { get => DpExpectedDate.SelectedDate; set => DpExpectedDate.SelectedDate = value; }
    public bool NormalizeCroatian => ChkCroatian.IsChecked == true;
    public string StatsText { get => LblNbStats.Text; set => LblNbStats.Text = value; }

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);
    private void NbIot_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFile(e);
        if (file is not null) InputFile = file;
    }
    private void Output_Drop(object sender, DragEventArgs e)
    {
        var folder = DragDropHelper.DroppedDirectoryOrParent(e);
        if (folder is not null) OutputFolder = folder;
    }

    private void BrowseNbIot_Click(object sender, RoutedEventArgs e) => BrowseNbIotRequested?.Invoke(this, EventArgs.Empty);
    private void RunNbIot_Click(object sender, RoutedEventArgs e) => RunRequested?.Invoke(this, EventArgs.Empty);
    private void OpenOutput_Click(object sender, RoutedEventArgs e) => OpenOutputRequested?.Invoke(this, EventArgs.Empty);
}
