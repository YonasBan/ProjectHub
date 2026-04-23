using ProjectHub.Application.Localization;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Splat;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 简单对话框 ViewModel 基类（无返回值）
/// </summary>
public abstract class DialogViewModelBase : ReactiveObject
{
    /// <summary>
    /// 关闭请求事件
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    /// <summary>
    /// 本地化字符串
    /// </summary>
    public LocalizedStrings L { get; }

    /// <summary>
    /// 关闭对话框
    /// </summary>
    protected void Close(bool confirmed = true)
    {
        CloseRequested?.Invoke(this, confirmed);
    }

    protected DialogViewModelBase()
    {
        L = Locator.Current.GetService<LocalizedStrings>()!;
    }
}

/// <summary>
/// 所有弹窗 ViewModel 的基类（带返回值）
/// </summary>
public abstract class DialogViewModelBase<TResult> : ReactiveObject, IDisposable, IDialogViewModel<TResult>
{
    private readonly TaskCompletionSource<DialogResult<TResult>> _tcs = new();
    public Task<DialogResult<TResult>> WaitForResultAsync() => _tcs.Task;

    /// <summary>
    /// 本地化字符串
    /// </summary>
    public LocalizedStrings L { get; }

    protected void Close(TResult result) =>
        _tcs.TrySetResult(new DialogResult<TResult>(true, result));

    public void Cancel() =>
        _tcs.TrySetResult(new DialogResult<TResult>(false));

    /// <summary>
    /// ReactiveUI 的 Dispose 容器
    /// 用于自动管理订阅的生命周期
    /// </summary>
    public CompositeDisposable Disposables { get; } = new();

    /// <summary>
    /// ReactiveCommand 方便绑定
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }


    protected DialogViewModelBase()
    {
        L = Locator.Current.GetService<LocalizedStrings>()!;
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        Disposables.Dispose();
    }

    public void CloseWindow()
    {
        _tcs.TrySetResult(new DialogResult<TResult>(false));
    }
}
