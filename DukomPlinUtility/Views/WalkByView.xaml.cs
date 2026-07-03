using System;
using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;

namespace DukomPlinUtility.Views;

public partial class WalkByView : UserControl
{
    public event EventHandler? BrowseXmlRequested;
    public event EventHandler? BrowseOutputRequested;
    public event EventHandler? OpenSettingsRequested;
    public event EventHandler? RunRequested;
    public event EventHandler? OpenOutputRequested;

    public WalkByView() => InitializeComponent();

    public string XmlFile { get => TxtXml.Text; set => TxtXml.Text = value; }
    public string OutputFolder { get => TxtOutput.Text; set => TxtOutput.Text = value; }
    public string StatsText { get => LblWalkByStats.Text; set => LblWalkByStats.Text = value; }

    public void SetSourceText(string source) => TxtWalkBySourceView.Text = source;

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);
    private void Xml_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFileByExtension(e, ".xml");
        if (file is not null) XmlFile = file;
    }
    private void Output_Drop(object sender, DragEventArgs e)
    {
        var folder = DragDropHelper.DroppedDirectoryOrParent(e);
        if (folder is not null) OutputFolder = folder;
    }

    private void BrowseXml_Click(object sender, RoutedEventArgs e) => BrowseXmlRequested?.Invoke(this, EventArgs.Empty);
    private void BrowseOutput_Click(object sender, RoutedEventArgs e) => BrowseOutputRequested?.Invoke(this, EventArgs.Empty);
    private void OpenSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
    private void RunWalkBy_Click(object sender, RoutedEventArgs e) => RunRequested?.Invoke(this, EventArgs.Empty);
    private void OpenOutput_Click(object sender, RoutedEventArgs e) => OpenOutputRequested?.Invoke(this, EventArgs.Empty);
}
