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
        DialogResult<TResult>? Result { get; }
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
       DialogResult<string> ShowInput(string title, string message,
            string? defaultValue = null);

        // 自定义 ViewModel 弹窗
       DialogResult<TResult> ShowDialog<TViewModel, TResult>(
            TViewModel viewModel)
            where TViewModel : IDialogViewModel<TResult>;
    }
}
