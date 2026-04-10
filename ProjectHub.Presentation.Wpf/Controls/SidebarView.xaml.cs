using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.ViewModels;
using ReactiveUI;

namespace ProjectHub.Presentation.Wpf.Controls;

/// <summary>
/// 侧边栏视图 (WPF 特定实现)
/// 
/// DDD 设计要点:
/// - 位于 Presentation 层，仅包含 WPF 特定代码
/// - 所有业务逻辑都在 MainViewModel.SidebarTreeItems 中
/// - 此文件内容在 MAUI/Avalonia 中需要重新实现，但 ViewModel 可以复用
/// </summary>
public partial class SidebarView :ReactiveUserControl<MainViewModel>
{
    public SidebarView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            // 订阅 TreeView 选中项变化事件，同步到 ViewModel
            Observable.FromEventPattern<RoutedPropertyChangedEventArgs<object>>(
                handler => NavigationTree.SelectedItemChanged += (s, e) => handler(s, e),
                handler => NavigationTree.SelectedItemChanged -= (s, e) => handler(s, e))
                .Subscribe(e =>
                {
                    if (ViewModel == null) ViewModel = this.DataContext as MainViewModel;
                    if (ViewModel != null && e.EventArgs.NewValue is TreeItemViewModel selectedItem)
                    {
                        ViewModel.SelectedTreeItem = selectedItem;
                    }
                })
                .DisposeWith(disposables);

            // 当 ViewModel 的 SelectedTreeItem 变化时，同步到 TreeView
            this.WhenAnyValue(v => v.ViewModel!.SelectedTreeItem)
                .Where(selected => selected != null)
                .Subscribe(selected =>
                {
                    if (selected != null)
                    {
                        // 使用 Dispatcher 确保在 UI 线程执行
                        Dispatcher.BeginInvoke(() =>
                        {
                            var container = FindTreeViewItem(NavigationTree, selected);
                            if (container != null)
                            {
                                container.IsSelected = true;
                                container.BringIntoView();
                            }
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                })
                .DisposeWith(disposables);
        });
    }


    /// <summary>
    /// 递归查找 TreeViewItem 容器
    /// </summary>
    private static TreeViewItem? FindTreeViewItem(ItemsControl container, object item)
    {
        if (container == null) return null;

        // 检查当前容器是否就是要找的项
        if (container is TreeViewItem tvi && tvi.DataContext == item)
            return tvi;

        // 展开容器以生成子项
        if (container is TreeViewItem parentItem && !parentItem.IsExpanded)
        {
            parentItem.IsExpanded = true;
            parentItem.UpdateLayout();
        }

        // 递归搜索子项
        for (int i = 0; i < container.Items.Count; i++)
        {
            var childContainer = (TreeViewItem?)container.ItemContainerGenerator.ContainerFromIndex(i);
            if (childContainer == null) continue;

            if (childContainer.DataContext == item)
                return childContainer;

            // 递归搜索子节点的子项
            var found = FindTreeViewItem(childContainer, item);
            if (found != null)
                return found;
        }

        return null;
    }
}
