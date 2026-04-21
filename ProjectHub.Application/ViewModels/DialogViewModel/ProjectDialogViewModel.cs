using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels.DialogViewModel;

/// <summary>
/// 项目对话框 ViewModel - 支持添加和编辑项目
/// </summary>
public class ProjectDialogViewModel : DialogViewModelBase<ProjectDto?>
{
    private readonly ILogger<ProjectDialogViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IFileAssociationService _fileAssociationService;
    private readonly IProjectAppService? _projectAppService;

    /// <summary>
    /// 对话框模式：添加或编辑
    /// </summary>
    public DialogMode Mode { get; private set; } = DialogMode.Add;

    /// <summary>
    /// 编辑时的项目ID
    /// </summary>
    public long? EditProjectId { get; private set; }

    /// <summary>
    /// 对话框标题
    /// </summary>
    public string DialogTitle => Mode == DialogMode.Add ? L.ProjectDialog_Title_Add : L.ProjectDialog_Title_Edit;

    /// <summary>
    /// 确认按钮文本
    /// </summary>
    public string ConfirmButtonText => Mode == DialogMode.Add ? L.Confirm : L.Save;

    private string _defaultProgram = string.Empty;
    public string DefaultProgram
    {
        get => _defaultProgram;
        set => this.RaiseAndSetIfChanged(ref _defaultProgram, value);
    }

    /// <summary>
    /// 启动参数（可选）
    /// </summary>
    private string _launchArguments = string.Empty;
    public string LaunchArguments
    {
        get => _launchArguments;
        set => this.RaiseAndSetIfChanged(ref _launchArguments, value);
    }

    /// <summary>
    /// 网页链接（可选）
    /// </summary>
    private string _webUrl = string.Empty;
    public string WebUrl
    {
        get => _webUrl;
        set => this.RaiseAndSetIfChanged(ref _webUrl, value);
    }

    /// <summary>
    /// CMD 命令（可选）
    /// </summary>
    private string _cmdCommand = string.Empty;
    public string CmdCommand
    {
        get => _cmdCommand;
        set => this.RaiseAndSetIfChanged(ref _cmdCommand, value);
    }

    /// <summary>
    /// 是否以管理员身份运行
    /// </summary>
    private bool _runAsAdmin;
    public bool RunAsAdmin
    {
        get => _runAsAdmin;
        set => this.RaiseAndSetIfChanged(ref _runAsAdmin, value);
    }

    /// <summary>
    /// 项目描述
    /// </summary>
    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// 项目图标路径（由 View 层负责加载显示）
    /// </summary>
    private string? _iconPath;
    public string? IconPath
    {
        get => _iconPath;
        private set => this.RaiseAndSetIfChanged(ref _iconPath, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        private set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    /// <summary>
    /// 是否为编辑模式（用于UI控制）
    /// </summary>
    public bool IsEditMode => Mode == DialogMode.Edit;

    /// <summary>
    /// 是否为添加模式（用于UI控制）
    /// </summary>
    public bool IsAddMode => Mode == DialogMode.Add;

    /// <summary>
    /// 确认命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    /// <summary>
    /// 浏览路径命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> BrowsePathCommand { get; }

    /// <summary>
    /// 浏览程序命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> BrowseProgramCommand { get; }

    /// <summary>
    /// 浏览图标命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> BrowseIconCommand { get; }

    /// <summary>
    /// 重置图标命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ResetIconCommand { get; }

    /// <summary>
    /// 项目名称
    /// </summary>
    private string _projectName = string.Empty;
    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    /// <summary>
    /// 项目路径
    /// </summary>
    private string _projectPath = string.Empty;
    public string ProjectPath
    {
        get => _projectPath;
        set => this.RaiseAndSetIfChanged(ref _projectPath, value);
    }

    /// <summary>
    /// 启动类型（用于 ComboBox 绑定）
    /// </summary>
    private LaunchType _launchType = LaunchType.OpenFile;
    public LaunchType LaunchType
    {
        get => _launchType;
        set => this.RaiseAndSetIfChanged(ref _launchType, value);
    }

    /// <summary>
    /// 启动类型列表（用于 ComboBox ItemsSource）
    /// </summary>
    public IReadOnlyList<LaunchTypeItem> LaunchTypeItems => new List<LaunchTypeItem>
    {
        new LaunchTypeItem(LaunchType.OpenFile, L.ProjectDialog_LaunchType_OpenFile),
        new LaunchTypeItem(LaunchType.OpenExe, L.ProjectDialog_LaunchType_OpenExe),
        new LaunchTypeItem(LaunchType.OpenWebUrl, L.ProjectDialog_LaunchType_OpenWebUrl),
        new LaunchTypeItem(LaunchType.OpenCmd, L.ProjectDialog_LaunchType_OpenCmd)
    };

    public ProjectDialogViewModel(
        ILogger<ProjectDialogViewModel> logger,
        IDialogService dialogService,
        IFileAssociationService fileAssociationService,
        IProjectAppService? projectAppService = null)
    {
        _logger = logger;
        _dialogService = dialogService;
        _fileAssociationService = fileAssociationService;
        _projectAppService = projectAppService;

        // 初始化命令
        BrowsePathCommand = ReactiveCommand.Create(BrowsePath);
        BrowseProgramCommand = ReactiveCommand.Create(BrowseProgram);
        BrowseIconCommand = ReactiveCommand.Create(BrowseIcon);
        ResetIconCommand = ReactiveCommand.Create(ResetIcon);

        // 响应式验证逻辑：当任何相关属性变化时自动重新计算错误信息和按钮状态
        var validationObservable = this.WhenAnyValue(
            x => x.ProjectName,
            x => x.ProjectPath,
            x => x.DefaultProgram,
            x => x.WebUrl,
            x => x.CmdCommand,
            x => x.LaunchType,
            (name, path, program, webUrl, cmdCommand, launchType) =>
            {
                // 项目名称必填
                if (string.IsNullOrWhiteSpace(name))
                    return L.ProjectDialog_Error_EmptyName;

                // 根据启动类型验证不同的字段
                return launchType switch
                {
                    LaunchType.OpenFile => string.IsNullOrWhiteSpace(path) ? L.ProjectDialog_Error_EmptyPath : null,
                    LaunchType.OpenExe => string.IsNullOrWhiteSpace(program) ? L.ProjectDialog_Error_EmptyProgram : null,
                    LaunchType.OpenWebUrl => 
                        string.IsNullOrWhiteSpace(webUrl) || webUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).Length == 0 
                            ? L.ProjectDialog_Error_EmptyWebUrl 
                            : null,
                    LaunchType.OpenCmd => string.IsNullOrWhiteSpace(cmdCommand) ? L.ProjectDialog_Error_EmptyCmdCommand : null,
                    _ => null
                };
            });

        // 订阅验证结果，更新错误信息
        validationObservable
            .Subscribe(errorMessage =>
            {
                ErrorMessage = errorMessage;
                HasError = !string.IsNullOrEmpty(errorMessage);
            })
            .DisposeWith(Disposables);

        // 确认按钮可用性：没有错误且项目名称不为空
        var canConfirm = this.WhenAnyValue(x => x.HasError, x => x.ProjectName, 
            (hasError, name) => !hasError && !string.IsNullOrWhiteSpace(name));
        
        ConfirmCommand = ReactiveCommand.CreateFromTask(ConfirmAsync, canConfirm);

        // 响应式图标更新逻辑
        this.WhenAnyValue(x => x.ProjectPath, x => x.DefaultProgram, x => x.LaunchType)
            .Subscribe(_ => UpdateIcon())
            .DisposeWith(Disposables);

        // 设置初始图标
        UpdateIcon();
    }

    /// <summary>
    /// 更新图标逻辑
    /// </summary>
    private void UpdateIcon()
    {
        if (!string.IsNullOrWhiteSpace(DefaultProgram))
        {
            IconPath = DefaultProgram;
        }
        else if (!string.IsNullOrWhiteSpace(ProjectPath))
        {
            IconPath = ProjectPath;
        }
        else
        {
            // 根据启动类型设置默认图标
            IconPath = LaunchType switch
            {
                LaunchType.OpenWebUrl => "Images/explorer.png",
                LaunchType.OpenCmd => "Images/CMD.png",
                _ => null
            };
        }
    }

    /// <summary>
    /// 初始化为添加模式
    /// </summary>
    public void InitializeForAdd()
    {
        Mode = DialogMode.Add;
        EditProjectId = null;
        ClearFields();
        this.RaisePropertyChanged(nameof(DialogTitle));
        this.RaisePropertyChanged(nameof(ConfirmButtonText));
        this.RaisePropertyChanged(nameof(IsEditMode));
        this.RaisePropertyChanged(nameof(IsAddMode));
    }

    /// <summary>
    /// 初始化为编辑模式
    /// </summary>
    public void InitializeForEdit(ProjectDto project)
    {
        Mode = DialogMode.Edit;
        EditProjectId = project.Id;

        ProjectName = project.Name;
        ProjectPath = project.Path;
        Description = project.Description ?? string.Empty;
        IconPath = project.CustomIconPath;

        // 从项目配置中设置启动类型
        LaunchType = project.LaunchType;
        DefaultProgram = project.DefaultProgram ?? string.Empty;
        LaunchArguments = project.LaunchArguments ?? string.Empty;
        WebUrl = project.WebUrl ?? string.Empty;
        CmdCommand = project.CmdCommand ?? string.Empty;
        RunAsAdmin = project.RunAsAdmin;

        this.RaisePropertyChanged(nameof(DialogTitle));
        this.RaisePropertyChanged(nameof(ConfirmButtonText));
        this.RaisePropertyChanged(nameof(IsEditMode));
        this.RaisePropertyChanged(nameof(IsAddMode));

        _logger.LogInformation($"初始化编辑模式，项目ID: {project.Id}");
    }

    private void ClearFields()
    {
        ProjectName = string.Empty;
        ProjectPath = string.Empty;
        LaunchType = LaunchType.OpenFile;
        DefaultProgram = string.Empty;
        LaunchArguments = string.Empty;
        WebUrl = string.Empty;
        CmdCommand = string.Empty;
        RunAsAdmin = false;
        Description = string.Empty;
        IconPath = null;
        ErrorMessage = null;
        HasError = false;
    }

    private void BrowsePath()
    {
        var path = _dialogService.ShowOpenFileDialog(
            "选择项目入口文件|*.*",
            "选择项目路径");

        if (!string.IsNullOrEmpty(path))
        {
            ProjectPath = path;

            // 自动提取项目名称（从文件名）
            if (string.IsNullOrWhiteSpace(ProjectName) || Mode == DialogMode.Add)
            {
                ProjectName = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            // 自动检测默认打开方式
            DetectDefaultProgram(path);

            _logger.LogInformation($"用户选择了路径: {path}");
        }
    }

    /// <summary>
    /// 自动检测文件的默认打开方式
    /// </summary>
    private void DetectDefaultProgram(string filePath)
    {
        try
        {
            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            // 根据文件类型设置默认打开方式
            string? defaultProgram = GetDefaultProgramFromRegistry(extension);
            if (!string.IsNullOrEmpty(defaultProgram))
            {
                DefaultProgram = defaultProgram;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"检测默认程序失败: {ex.Message}");
        }
    }
    /// <summary>
    /// 从注册表获取默认程序（通过接口）
    /// </summary>
    private string? GetDefaultProgramFromRegistry(string extension)
    {
        return _fileAssociationService.GetDefaultProgramByExtension(extension);
    }

    private void BrowseProgram()
    {
        var program = _dialogService.ShowOpenFileDialog(
            "选择程序|*.exe;*.bat;*.cmd;*.ps1|所有文件|*.*",
            "选择默认程序");

        if (!string.IsNullOrEmpty(program))
        {
            DefaultProgram = program;
            _logger.LogInformation($"用户选择了程序: {program}");
        }
    }

    private void BrowseIcon()
    {
        var iconPath = _dialogService.ShowOpenFileDialog(
            "选择图标|*.ico;*.png;*.jpg;*.jpeg|所有文件|*.*",
            "选择图标文件");

        if (!string.IsNullOrEmpty(iconPath))
        {
            IconPath = iconPath;
            _logger.LogInformation($"用户选择了图标: {iconPath}");
        }
    }

    private void ResetIcon()
    {
        UpdateIcon();
        _logger.LogInformation("重置图标为默认");
    }

    private async Task ConfirmAsync()
    {
        if (HasError || string.IsNullOrWhiteSpace(ProjectName))
        {
            return;
        }

        if (Mode == DialogMode.Add)
        {
            _logger.LogInformation($"确认添加项目: {ProjectName}");

            // 创建 CreateProjectDto，根据启动类型设置相应字段
            var dto = new CreateProjectDto
            {
                Name = ProjectName,
                Description = Description,
                CustomIconPath = IconPath,
                LaunchType = LaunchType,
                // 根据启动类型设置不同的路径字段
                Path = LaunchType == LaunchType.OpenFile ? ProjectPath : string.Empty,
                // 打开文件时，自定义程序和启动参数可选；打开 exe 时必填
                DefaultProgram = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe)
                    ? (string.IsNullOrWhiteSpace(DefaultProgram) ? null : DefaultProgram)
                    : null,
                LaunchArguments = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe)
                    ? (string.IsNullOrWhiteSpace(LaunchArguments) ? null : LaunchArguments)
                    : null,
                WebUrl = LaunchType == LaunchType.OpenWebUrl ? (string.IsNullOrWhiteSpace(WebUrl) ? null : WebUrl) : null,
                CmdCommand = LaunchType == LaunchType.OpenCmd ? (string.IsNullOrWhiteSpace(CmdCommand) ? null : CmdCommand) : null,
                RunAsAdmin = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe) ? RunAsAdmin : false
            };

            if (_projectAppService != null)
            {
                var result = await _projectAppService.CreateAsync(dto);
                Close(result);
            }
            else
            {
                // 如果没有服务，返回 null（用于测试场景）
                Close(null);
            }
        }
        else
        {
            _logger.LogInformation($"确认编辑项目: {ProjectName}, ID: {EditProjectId}");

            if (_projectAppService != null && EditProjectId.HasValue)
            {
                // 创建 UpdateProjectDto，根据启动类型设置相应字段
                var updateDto = new UpdateProjectDto
                {
                    Id = EditProjectId.Value,
                    Name = ProjectName,
                    Description = Description,
                    CustomIconPath = IconPath,
                    LaunchType = LaunchType,
                    // 根据启动类型设置不同的路径字段
                    Path = LaunchType == LaunchType.OpenFile ? ProjectPath : string.Empty,
                    // 打开文件时，自定义程序和启动参数可选；打开 exe 时必填
                    DefaultProgram = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe)
                        ? (string.IsNullOrWhiteSpace(DefaultProgram) ? null : DefaultProgram)
                        : null,
                    LaunchArguments = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe)
                        ? (string.IsNullOrWhiteSpace(LaunchArguments) ? null : LaunchArguments)
                        : null,
                    WebUrl = LaunchType == LaunchType.OpenWebUrl ? (string.IsNullOrWhiteSpace(WebUrl) ? null : WebUrl) : null,
                    CmdCommand = LaunchType == LaunchType.OpenCmd ? (string.IsNullOrWhiteSpace(CmdCommand) ? null : CmdCommand) : null,
                    RunAsAdmin = (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe) ? RunAsAdmin : false
                };

                var result = await _projectAppService.UpdateAsync(updateDto);
                Close(result);
            }
            else
            {
                Close(null);
            }
        }
    }
}

/// <summary>
/// 对话框模式
/// </summary>
public enum DialogMode
{
    Add,
    Edit
}

/// <summary>
/// 启动类型显示项（用于 ComboBox）
/// </summary>
public class LaunchTypeItem
{
    public LaunchType Type { get; }
    public string DisplayName { get; }

    public LaunchTypeItem(LaunchType type, string displayName)
    {
        Type = type;
        DisplayName = displayName;
    }
}
