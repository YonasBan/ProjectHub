using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Reactive;
using ProjectHub.Core.Exceptions;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// ViewModel 基类
/// 所有 ViewModel 应继承此类，提供通用功能
/// 
/// DDD 设计要点:
/// - 使用 ReactiveUI (同时支持 WPF 和 Avalonia)
/// - 不直接引用 WPF/Avalonia API，便于迁移
/// - 通过接口与领域层交互 (依赖注入)
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger Logger;

    protected ViewModelBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// 从异步函数创建 ReactiveCommand
    /// </summary>
    protected static ReactiveCommand<Unit, Unit> CreateCommand(Func<Task> executeAction)
    {
        return ReactiveCommand.CreateFromTask(executeAction);
    }

    /// <summary>
    /// 创建带有 canExecute 可观察对象的 ReactiveCommand
    /// </summary>
    protected static ReactiveCommand<Unit, Unit> CreateCommand(Func<Task> executeAction, IObservable<bool>? canExecute = null)
    {
        return ReactiveCommand.CreateFromTask(executeAction, canExecute);
    }
}
