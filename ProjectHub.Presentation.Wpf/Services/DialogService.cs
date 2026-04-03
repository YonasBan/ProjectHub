using System.Windows;
using ProjectHub.Application.Interfaces;
using ProjectHub.Presentation.Wpf.Dialogs;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// WPF 对话框服务实现
/// 
/// 跨平台设计要点:
/// - 平台特定实现，放在 Presentation 层
/// - 抽象接口 IDialogService 定义在 Application 层
/// - 未来可替换为 MAUI/Avalonia 实现
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// 获取当前激活的窗口（用于对话框 Owner）
    /// </summary>
    private Window? GetActiveWindow()
    {
        return System.Windows.Application.Current?.MainWindow?.IsActive == true
            ? System.Windows.Application.Current.MainWindow
            : System.Windows.Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }

    public Task ShowMessageAsync(string title, string message, string? okText = null)
    {
        var button = MessageBoxButton.OK;
        var image = MessageBoxImage.Information;

        // 注意：MessageBox 不支持自定义按钮文本，这是 WPF 限制
        MessageBox.Show(GetActiveWindow(), message, title, button, image);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
    {
        var result = MessageBox.Show(
            GetActiveWindow(),
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public async Task<DialogResult<string>> ShowInputAsync(string title, string message, string? defaultValue = null)
    {
        // 使用 InputDialog 作为通用输入对话框
        var dialog = new InputDialog();

        // 获取父窗口作为 Owner
        var owner = GetActiveWindow();
        if (owner != null)
        {
            dialog.Owner = owner;
        }

        dialog.ShowDialog();

        // 返回结果
        if (dialog.DataContext is IDialogViewModel<string> vm)
        {
            var result =  vm.Result;
            return new DialogResult<string>(result.Confirmed, result.Value);
        }

        return new DialogResult<string>(false, defaultValue);
    }

    public  DialogResult<TResult> ShowDialog<TViewModel, TResult>(TViewModel viewModel)
        where TViewModel : IDialogViewModel<TResult>
    {
        var dialog = new InputDialog
        {
            DataContext = viewModel
        };

        // 获取父窗口作为 Owner
        var owner = GetActiveWindow();
        if (owner != null)
        {
            dialog.Owner = owner;
        }

        dialog.ShowDialog();

        // 返回结果
        var result =  viewModel.Result;
        return new DialogResult<TResult>(result.Confirmed, result.Value);
    }
}
