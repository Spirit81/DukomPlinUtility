using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;
using DukomPlinUtility.ViewModels;

namespace DukomPlinUtility.Views;

public partial class WalkByView : UserControl
{
    public WalkByView() => InitializeComponent();

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);

    private void Xml_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFileByExtension(e, ".xml");
        if (file is not null && DataContext is WalkByViewModel vm)
        {
            vm.SetDroppedXml(file);
        }
    }

    private void Output_Drop(object sender, DragEventArgs e)
    {
        var folder = DragDropHelper.DroppedDirectoryOrParent(e);
        if (folder is not null && DataContext is WalkByViewModel vm)
        {
            vm.SetDroppedOutput(folder);
        }
    }
}
