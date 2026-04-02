using System.Windows;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// WPF 对话框服务实现
/// </summary>
public class DialogService : IDialogService
{
    public Task ShowMessageAsync(string title, string message, string? okText = null)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<DialogResult<string>> ShowInputAsync(string title, string message, string? defaultValue = null)
    {
        
        return Task.FromResult(new DialogResult<string>(false, defaultValue));
    }

    public Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel)
        where TViewModel : IDialogViewModel<TResult>
    {
        // TODO: 实现自定义对话框
        return Task.FromResult(new DialogResult<TResult>(false));
    }
}
