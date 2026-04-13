using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels;
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

}
