using ProjectHub.Application.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProjectHub.Presentation.Wpf.Dialogs;

/// <summary>
/// 文件夹选择对话框
/// </summary>
public partial class SelectFolderDialog : Window
{
    public SelectFolderDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 树形选择改变事件
    /// </summary>
    private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is SelectFolderDialogViewModel viewModel && e.NewValue is FolderTreeItemViewModel selectedItem)
        {
            viewModel.SelectedFolder = selectedItem;
        }
    }
}
