using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.ViewModels;

namespace ProjectHub.Presentation.Wpf.Controls;

/// <summary>
/// 侧边栏视图 (WPF 特定实现)
/// 
/// DDD 设计要点:
/// - 位于 Presentation 层，仅包含 WPF 特定代码
/// - 所有业务逻辑都在 MainViewModel.SidebarTreeItems 中
/// - 此文件内容在 MAUI/Avalonia 中需要重新实现，但 ViewModel 可以复用
/// </summary>
public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// 鼠标悬停事件 - 显示操作按钮
    /// 
    /// 注意：这是平台特定的 UI 交互，在 MAUI/Avalonia 中可能需要不同的实现方式
    /// </summary>
    private void ItemBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border { Tag: TreeItemViewModel item })
            item.IsHovered = true;
    }

    /// <summary>
    /// 鼠标离开事件 - 隐藏操作按钮
    /// </summary>
    private void ItemBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border { Tag: TreeItemViewModel item })
            item.IsHovered = false;
    }
}
