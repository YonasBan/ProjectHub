using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectHub.Application.Interfaces
{
    // 基础弹窗结果
    public record DialogResult<T>(bool Confirmed, T? Value = default);

    // 对话框 ViewModel 接口
    public interface IDialogViewModel<TResult>
    {
        Task<DialogResult<TResult>> WaitForResultAsync();
    }

    // 核心弹窗服务接口
    public interface IDialogService
    {
        // 消息提示
        Task ShowMessageAsync(string title, string message, string? okText = null);

        // 确认对话框
        Task<bool> ShowConfirmAsync(string title, string message,
            string? confirmText = null, string? cancelText = null);

        // 输入框
       Task<DialogResult<string>> ShowInputAsync(string title, string message,
            string? defaultValue = null);

        // 打开文件对话框
        string? ShowOpenFileDialog(string filter, string title);

        // 自定义 ViewModel 弹窗（传入ViewModel实例）
        Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>(
            TViewModel viewModel)
            where TViewModel : IDialogViewModel<TResult>;

        // 自定义 ViewModel 弹窗（通过IoC获取ViewModel）
        Task<DialogResult<TResult>> ShowDialogAsync<TViewModel,TResult>()
            where TViewModel : IDialogViewModel<TResult>;
    }
}
