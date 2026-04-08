using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 所有弹窗 ViewModel 的基类
/// </summary>
public abstract class DialogViewModelBase<TResult> : ReactiveObject, IDisposable, IDialogViewModel<TResult>
{
    private readonly TaskCompletionSource<DialogResult<TResult>> _tcs = new();
    public Task<DialogResult<TResult>> WaitForResultAsync() => _tcs.Task;

    protected void Close(TResult result) =>
        _tcs.TrySetResult(new DialogResult<TResult>(true, result));

    protected void Cancel() =>
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

    public DialogResult<TResult>? Result => throw new NotImplementedException();

    protected DialogViewModelBase()
    {
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        Disposables.Dispose();
    }
}
