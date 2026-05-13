using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class HeaderView : UserControl
{

    public HeaderView()
    {
        InitializeComponent();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.ContextMenu != null)
        {
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = true;
        }
    }

    private void MinimizeToTrayMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // 通过数据绑定自动更新 ViewModel 中的 MinimizeToTray 属性
        // 该属性的 setter 会自动保存配置
        if (DataContext is MainViewModel viewModel && sender is MenuItem menuItem)
        {
            viewModel.MinimizeToTray = menuItem.IsChecked;
        }
    }
}
