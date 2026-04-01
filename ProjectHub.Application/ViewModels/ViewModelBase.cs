using Microsoft.Extensions.Logging;
using ProjectHub.Application.Interfaces;
using ProjectHub.Core.Exceptions;
using ProjectHub.Resources.Strings;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;

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
public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// 主线程调度器 (由平台层提供,用于跨平台兼容)
    /// </summary>
    protected readonly IScheduler MainThreadScheduler;
    private readonly ILocalizationService localizationService;

    protected ViewModelBase(ILogger logger, IScheduler mainThreadScheduler, ILocalizationService localizationService)
    {
        Logger = logger;
        MainThreadScheduler = mainThreadScheduler;
        this.localizationService = localizationService;
        localizationService.CultureChanged
          .ObserveOn(mainThreadScheduler)
          .Subscribe(_ =>

          {
              this.RaisePropertyChanged(string.Empty);
              
          }

          )
          .DisposeWith(Disposables);
    }

    /// <summary>
    /// ReactiveUI 的 Dispose 容器
    /// 用于自动管理订阅的生命周期
    /// </summary>
    public CompositeDisposable Disposables { get; } = new();

    public ViewModelActivator Activator => throw new NotImplementedException();

    /// <summary>
    /// 从异步函数创建 ReactiveCommand（使用主线程调度器）
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateCommand(Func<Task> executeAction)
    {
        return ReactiveCommand.CreateFromTask(
            executeAction,
            outputScheduler: MainThreadScheduler);
    }

    /// <summary>
    /// 创建带有 canExecute 可观察对象的 ReactiveCommand（使用主线程调度器）
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateCommand(Func<Task> executeAction, IObservable<bool>? canExecute = null)
    {
        return ReactiveCommand.CreateFromTask(
            executeAction,
            canExecute,
            outputScheduler: MainThreadScheduler);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Deactivate()
    {
        Disposables.Dispose();
    }
}
