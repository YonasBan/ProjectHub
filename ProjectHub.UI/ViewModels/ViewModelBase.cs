using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Reactive;
using ProjectHub.Core.Exceptions;

namespace ProjectHub.UI.ViewModels;

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
    /// 页面标题
    /// </summary>
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// 是否正在加载
    /// </summary>
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// 是否有错误
    /// </summary>
    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger Logger;

    protected ViewModelBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Executes an action asynchronously with error handling
    /// </summary>
    protected async Task ExecuteWithErrorHandlerAsync(Func<Task> action, string operationName)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            await action();
        }
        catch (ValidationException ex)
        {
            Logger.LogWarning(ex, "Validation exception: {Operation}", operationName);
            HasError = true;
            ErrorMessage = string.Join(", ", ex.Errors.SelectMany(kvp => kvp.Value));
        }
        catch (DomainException ex)
        {
            Logger.LogError(ex, "Domain exception: {Operation}", operationName);
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unknown exception: {Operation}", operationName);
            HasError = true;
            ErrorMessage = $"Operation failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
