using System.IO;
using System.Windows;
using System.Windows.Controls;
using DukomPlinUtility.Helpers;
using DukomPlinUtility.ViewModels;

namespace DukomPlinUtility.Views;

public partial class ZgradeView : UserControl
{
    public ZgradeView() => InitializeComponent();

    private void File_DragOver(object sender, DragEventArgs e) => DragDropHelper.SetFileDropEffect(e);

    private void Zgrade_Drop(object sender, DragEventArgs e)
    {
        if (DataContext is not ZgradeViewModel vm) return;

        foreach (var file in DragDropHelper.GetDroppedFiles(e).Where(File.Exists))
        {
            vm.AddFile(file);
        }
    }
}
