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

    private string _projectName = string.Empty;
    public string ProjectName
    {
        get => _projectName;
        set
        {
            this.RaiseAndSetIfChanged(ref _projectName, value);
            ValidateInput();
        }
    }

    private string _projectPath = string.Empty;
    public string ProjectPath
    {
        get => _projectPath;
        set
        {
            this.RaiseAndSetIfChanged(ref _projectPath, value);
            ValidateInput();
            UpdateIconFromPath();
        }
    }

    /// <summary>
    /// 启动类型（用于 ComboBox 绑定）
    /// </summary>
    private LaunchType _launchType = LaunchType.OpenFile;
    public LaunchType LaunchType
    {
        get => _launchType;
        set
        {
            this.RaiseAndSetIfChanged(ref _launchType, value);
            ValidateInput();
            // 切换启动类型时更新默认图标
            if (string.IsNullOrWhiteSpace(ProjectPath) && string.IsNullOrWhiteSpace(DefaultProgram))
            {
                SetDefaultIcon();
            }
        }
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


    private string _defaultProgram = string.Empty;
    public string DefaultProgram
    {
        get => _defaultProgram;
        set
        {
            this.RaiseAndSetIfChanged(ref _defaultProgram, value);
            ValidateInput();
            UpdateIconFromProgram();
        }
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

        // 根据启动类型动态确定确认按钮是否可用
        var canConfirm = this.WhenAnyValue(
            x => x.ProjectName,
            x => x.ProjectPath,
            x => x.DefaultProgram,
            x => x.WebUrl,
            x => x.CmdCommand,
            x => x.LaunchType,
            (name, path, program, webUrl, cmdCommand, launchType) =>
            {
                // 项目名称始终必填
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                // 根据启动类型验证不同字段
                return launchType switch
                {
                    LaunchType.OpenFile => !string.IsNullOrWhiteSpace(path),  // 打开文件：路径必填
                    LaunchType.OpenExe => !string.IsNullOrWhiteSpace(program), // 打开 exe：程序必填
                    LaunchType.OpenWebUrl => !string.IsNullOrWhiteSpace(webUrl) && webUrl.Split(';', StringSplitOptions.RemoveEmptyEntries).Length > 0, // 打开网页：链接必填，支持多个
                    LaunchType.OpenCmd => !string.IsNullOrWhiteSpace(cmdCommand), // 运行 CMD：命令必填
                    _ => false
                };
            });
        ConfirmCommand = ReactiveCommand.Create(Confirm, canConfirm);

        // 设置默认图标
        SetDefaultIcon();
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

    private void ValidateInput()
    {
        // 项目名称必填
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ErrorMessage = L.ProjectDialog_Error_EmptyName;
            HasError = true;
            return;
        }

        // 根据启动类型验证不同的字段
        switch (LaunchType)
        {
            case LaunchType.OpenFile:
                // 打开文件：项目路径必填，自定义程序和启动参数可选
                if (string.IsNullOrWhiteSpace(ProjectPath))
                {
                    ErrorMessage = L.ProjectDialog_Error_EmptyPath;
                    HasError = true;
                    return;
                }
                break;

            case LaunchType.OpenExe:
                // 打开 exe：程序路径必填
                if (string.IsNullOrWhiteSpace(DefaultProgram))
                {
                    ErrorMessage = L.ProjectDialog_Error_EmptyProgram;
                    HasError = true;
                    return;
                }
                break;

            case LaunchType.OpenWebUrl:
                // 打开网页：网页链接必填，支持多个链接用分号分隔
                if (string.IsNullOrWhiteSpace(WebUrl))
                {
                    ErrorMessage = L.ProjectDialog_Error_EmptyWebUrl;
                    HasError = true;
                    return;
                }
                // 验证至少有一个有效的链接
                var urls = WebUrl.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (urls.Length == 0 || urls.All(string.IsNullOrWhiteSpace))
                {
                    ErrorMessage = L.ProjectDialog_Error_EmptyWebUrl;
                    HasError = true;
                    return;
                }
                break;

            case LaunchType.OpenCmd:
                // 运行 CMD：命令必填
                if (string.IsNullOrWhiteSpace(CmdCommand))
                {
                    ErrorMessage = L.ProjectDialog_Error_EmptyCmdCommand;
                    HasError = true;
                    return;
                }
                break;
        }

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

            // 更新图标
            UpdateIconFromPath();

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

    private void UpdateIconFromPath()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            SetDefaultIcon();
            return;
        }

        // 返回文件路径，由 View 层提取图标
        IconPath = ProjectPath;
    }

    private void UpdateIconFromProgram()
    {
        if (string.IsNullOrWhiteSpace(DefaultProgram))
        {
            return;
        }

        // 优先使用程序图标
        IconPath = DefaultProgram;
    }

    private void ResetIcon()
    {
        UpdateIconFromPath();
        _logger.LogInformation("重置图标为默认");
    }

    private void SetDefaultIcon()
    {
        // 根据启动类型设置默认图标
        IconPath = LaunchType switch
        {
            LaunchType.OpenWebUrl => "Images/explorer.png",
            LaunchType.OpenCmd=> "Images/CMD.png",
            _ => null
        };
    }

    private async void Confirm()
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
