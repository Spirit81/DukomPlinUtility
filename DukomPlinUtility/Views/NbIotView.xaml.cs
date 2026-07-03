using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;
using DukomPlinUtility.ViewModels;

namespace DukomPlinUtility.Views;

public partial class NbIotView : UserControl
{
    public NbIotView() => InitializeComponent();

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);

    private void NbIot_Drop(object sender, DragEventArgs e)
    {
        var file = DragDropHelper.FirstFile(e);
        if (file is not null && DataContext is NbIotViewModel vm)
        {
            vm.SetDroppedInput(file);
        }
    }

    private void Output_Drop(object sender, DragEventArgs e)
    {
        var folder = DragDropHelper.DroppedDirectoryOrParent(e);
        if (folder is not null && DataContext is NbIotViewModel vm)
        {
            vm.SetDroppedOutput(folder);
        }
    }
}
