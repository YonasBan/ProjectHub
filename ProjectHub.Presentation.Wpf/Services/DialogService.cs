using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels.DialogViewModel;
using ProjectHub.Presentation.Wpf.Controls;
using ProjectHub.Presentation.Wpf.Dialogs;
using ReactiveUI;
using System;
using System.Reactive.Concurrency;
using System.Windows;

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
    private readonly IServiceProvider _serviceProvider;
    private readonly IScheduler mainThreadScheduler;

    public DialogService(IServiceProvider serviceProvider, IScheduler mainThreadScheduler)
    {
        _serviceProvider = serviceProvider;
        this.mainThreadScheduler = mainThreadScheduler;
    }
    /// <summary>
    /// 获取当前激活的窗口（用于对话框 Owner）
    /// </summary>
    private Window? GetActiveWindow()
    {
        return System.Windows.Application.Current.MainWindow;

    }

    public Task ShowMessageAsync(string title, string message, string? okText = null)
    {
        var button = MessageBoxButton.OK;
        var image = MessageBoxImage.Information;

        // 注意：MessageBox 不支持自定义按钮文本，这是 WPF 限制
        MessageBox.Show(GetActiveWindow(), message, title, button, image);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 显示通知提示（自动消失的弹出提示）
    /// </summary>
    public void ShowNotification(string message, NotificationType type = NotificationType.Info, int durationMs = 3000)
    {
        // 在 UI 线程执行
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastNotification(message, type, durationMs);
            toast.Show();
        });
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

    public Task<DialogResult<string>> ShowInputAsync(string title, string message, string? defaultValue = null)
    {
        throw new NotImplementedException();
    }

    public string? ShowOpenFileDialog(string filter, string title)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            CheckFileExists = true,
            CheckPathExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSelectFolderDialog(string title)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true
        };

        // 使用 WPF 窗口句柄作为父窗口
        var owner = GetActiveWindow();
        if (owner != null)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(owner);
            // FolderBrowserDialog 在 .NET Core/WPF 中需要通过 Win32 API 设置 Owner，这里简化处理
        }

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }

    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel)
        where TViewModel : IDialogViewModel<TResult>
    {
        return await ShowDialogInternalAsync<TViewModel, TResult>(viewModel);
    }

    /// <inheritdoc />
    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : IDialogViewModel<TResult>
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        return await ShowDialogInternalAsync<TViewModel, TResult>(viewModel);
    }

    private async Task<DialogResult<TResult>> ShowDialogInternalAsync<TViewModel, TResult>(TViewModel viewModel)
        where TViewModel : IDialogViewModel<TResult>
    {
        var view = ViewLocator.Current.ResolveView(viewModel)
        ?? throw new InvalidOperationException(
            $"未找到 {typeof(TViewModel).Name} 对应的 View，" +
            $"请确认 View 实现了 IViewFor<{typeof(TViewModel).Name}>");
        if (view is not Window window)
            throw new InvalidOperationException("Dialog 的 View 必须是 Window");
        // 获取父窗口作为 Owner
        try
        {
            var owner = GetActiveWindow();
            // ✅ 加这一行！否则 ViewModel 是 null
            view.ViewModel = viewModel;
            if (owner != null)
            {
                window.Owner = owner;
            }

            window.Show();
            window.Closed += (s, e) =>
            {
                // 确保 ViewModel 收到关闭通知
                viewModel.CloseWindow();
            };
            // 返回结果
            var result = await viewModel.WaitForResultAsync();

            return new DialogResult<TResult>(result.Confirmed, result.Value);
        }
        finally
        {
            await window.Dispatcher.InvokeAsync(() =>
            {
                if (window.IsLoaded)
                    window.Close();
            });
            // 释放 ViewModel 所有订阅
            if (viewModel is IDisposable disposable)
                disposable.Dispose();
        }

    }

    /// <summary>
    /// 显示对话框（返回是否确认）
    /// </summary>
    public async Task<bool> ShowDialogAsync(DialogViewModelBase viewModel)
    {
        // 根据 ViewModel 类型创建对应的对话框窗口
        Window? window = viewModel switch
        {
            SelectFolderDialogViewModel => new SelectFolderDialog(),
            _ => throw new NotSupportedException($"不支持的对话框类型: {viewModel.GetType().Name}")
        };

        if (window == null)
            throw new InvalidOperationException("无法创建对话框窗口");

        // 设置数据上下文
        window.DataContext = viewModel;

        // 获取父窗口
        var owner = GetActiveWindow();
        if (owner != null)
        {
            window.Owner = owner;
        }

        // 创建任务完成源来等待对话框结果
        var tcs = new TaskCompletionSource<bool>();

        // 订阅 ViewModel 的关闭事件
        viewModel.CloseRequested += async (sender, confirmed) =>
        {
            // 对于 SelectFolderDialogViewModel，需要执行异步确认操作
            if (viewModel is SelectFolderDialogViewModel folderVm && confirmed)
            {
                var success = await folderVm.ConfirmAsync();
                if (!success)
                {
                    // 确认失败（如验证错误），不关闭对话框
                    return;
                }
            }

            tcs.TrySetResult(confirmed);
            window.Close();
        };

        // 显示对话框（模态）
        window.ShowDialog();

        return await tcs.Task;
    }

}
