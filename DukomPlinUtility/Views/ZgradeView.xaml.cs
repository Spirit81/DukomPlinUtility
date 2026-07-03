using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;

namespace DukomPlinUtility.Views;

public partial class ZgradeView : UserControl
{
    public event EventHandler? BrowseFilesRequested;
    public event EventHandler? ClearRequested;
    public event EventHandler? RunRequested;
    public event EventHandler<string>? FileDropped;

    public ZgradeView() => InitializeComponent();

    public IEnumerable? FilesSource { get => ListZgradeFiles.ItemsSource; set => ListZgradeFiles.ItemsSource = value; }
    public IEnumerable? IssuesSource { get => GridZgradeIssues.ItemsSource; set => GridZgradeIssues.ItemsSource = value; }
    public string StatsText { get => LblZgradeStats.Text; set => LblZgradeStats.Text = value; }

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);
    private void Zgrade_Drop(object sender, DragEventArgs e)
    {
        foreach (var file in DragDropHelper.GetDroppedFiles(e).Where(File.Exists))
        {
            FileDropped?.Invoke(this, file);
        }
    }

    private void BrowseZgrade_Click(object sender, RoutedEventArgs e) => BrowseFilesRequested?.Invoke(this, EventArgs.Empty);
    private void ClearZgrade_Click(object sender, RoutedEventArgs e) => ClearRequested?.Invoke(this, EventArgs.Empty);
    private void RunZgrade_Click(object sender, RoutedEventArgs e) => RunRequested?.Invoke(this, EventArgs.Empty);
}
