using Microsoft.Extensions.Logging;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Runtime.InteropServices.JavaScript;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
    public LocalizedStrings L { get; }
    
    private readonly ILocalizationService? _localizationService;
    
    /// <summary>
    /// 主线程调度器 (由平台层提供，用于跨平台兼容)
    /// </summary>
    protected readonly IScheduler MainThreadScheduler;

    protected ViewModelBase(ILogger logger, IScheduler mainThreadScheduler)
    {
        Logger = logger;
        MainThreadScheduler = mainThreadScheduler;
        _localizationService = Locator.Current.GetService<ILocalizationService>();
        L = Locator.Current.GetService<LocalizedStrings>()!;
        
        // 订阅语言改变事件，触发 UI 刷新
        _localizationService?.CultureChanged
            .ObserveOn(MainThreadScheduler) // 确保在 UI 线程执行
            .Subscribe(_ => this.RaisePropertyChanged(nameof(L)))
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
