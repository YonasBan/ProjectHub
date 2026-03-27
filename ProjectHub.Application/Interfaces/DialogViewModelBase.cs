using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace ProjectHub.Application.Interfaces
{
    // 所有弹窗 ViewModel 的基类
    public abstract class DialogViewModelBase<TResult> : ReactiveObject
    {
        private readonly TaskCompletionSource<DialogResult<TResult>> _tcs = new();

        public Task<DialogResult<TResult>> Result => _tcs.Task;

        protected void Close(TResult result) =>
            _tcs.TrySetResult(new DialogResult<TResult>(true, result));

        protected void Cancel() =>
            _tcs.TrySetResult(new DialogResult<TResult>(false));

        // ReactiveCommand 方便绑定
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        protected DialogViewModelBase()
        {
            CancelCommand = ReactiveCommand.Create(Cancel);
        }
    }
}
