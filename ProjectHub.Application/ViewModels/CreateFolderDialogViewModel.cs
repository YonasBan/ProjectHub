using Microsoft.Extensions.Logging;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 创建文件夹对话框 ViewModel
/// 
/// DDD 设计要点:
/// - 纯业务逻辑，不包含 UI 操作
/// - 使用 ReactiveUI 实现响应式验证
/// - 支持跨平台（MAUI/Avalonia）
/// </summary>
public class CreateFolderDialogViewModel : DialogViewModelBase<string>
{
    private readonly ILogger _logger;
    private readonly LocalizedStrings L;

    /// <summary>
    /// 文件夹名称
    /// </summary>
    private string _folderName = string.Empty;
    public string FolderName
    {
        get => _folderName;
        set => this.RaiseAndSetIfChanged(ref _folderName, value);
    }

    /// <summary>
    /// 验证错误消息
    /// </summary>
    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// 是否有错误
    /// </summary>
    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        private set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    /// <summary>
    /// 对话框标题
    /// </summary>
    public string Title => L.Sidebar_NewFolder;

    /// <summary>
    /// 文件夹名称占位符文本
    /// </summary>
    public string FolderNamePlaceholder => L.Folder_NamePlaceholder;

    /// <summary>
    /// 输入标签文本
    /// </summary>
    public string InputLabel => L.Folder_InputLabel;

    /// <summary>
    /// 确认按钮文本
    /// </summary>
    public string ConfirmText => L.Confirm;

    /// <summary>
    /// 取消按钮文本
    /// </summary>
    public string CancelText => L.Cancel;

    /// <summary>
    /// 默认文件夹名称（用于预填充）
    /// </summary>
    public string DefaultFolderName { get; }

    /// <summary>
    /// 确认命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    /// <summary>
    /// 验证并确认
    /// </summary>
    private void ExecuteConfirm()
    {
        if (Validate())
        {
            _logger.LogInformation("创建文件夹: {FolderName}", FolderName);
            Close(FolderName.Trim());
        }
    }

    /// <summary>
    /// 验证输入
    /// </summary>
    private bool Validate()
    {
        HasError = false;
        ErrorMessage = null;

        var name = FolderName?.Trim() ?? string.Empty;

        // 检查是否为空
        if (string.IsNullOrWhiteSpace(name))
        {
            HasError = true;
            ErrorMessage = L.Folder_Error_EmptyName;
            return false;
        }

        // 检查长度
        if (name.Length > 100)
        {
            HasError = true;
            ErrorMessage = L.Folder_Error_NameTooLong;
            return false;
        }

        // 检查非法字符
        var invalidChars = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        if (name.Any(c => invalidChars.Contains(c)))
        {
            HasError = true;
            ErrorMessage = L.Folder_Error_InvalidChars;
            return false;
        }

        return true;
    }

    public CreateFolderDialogViewModel(
        ILogger logger,
        LocalizedStrings l,
        IScheduler mainThreadScheduler,
        string? defaultName = null)
        : base()
    {
        _logger = logger;
        this.L = l;
        DefaultFolderName = defaultName ?? string.Empty;
        _folderName = DefaultFolderName;

        // 创建确认命令，带验证
        var canConfirm = this.WhenAnyValue(x => x.FolderName)
            .Select(name => !string.IsNullOrWhiteSpace(name)).ObserveOn(mainThreadScheduler);

        ConfirmCommand = ReactiveCommand.Create(ExecuteConfirm, canConfirm);

        // 监听 FolderName 变化，清除错误
        this.WhenAnyValue(x => x.FolderName).ObserveOn(mainThreadScheduler)
            .Subscribe(_ =>
            {
                if (HasError)
                {
                    HasError = false;
                    ErrorMessage = null;
                }
            })
            .DisposeWith(Disposables);
    }
}
