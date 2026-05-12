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
    private bool _isIconCustomized = false;

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
    /// 文件的关联程序列表
    /// </summary>
    private IReadOnlyList<AssociatedProgram> _associatedPrograms = Array.Empty<AssociatedProgram>();
    public IReadOnlyList<AssociatedProgram> AssociatedPrograms
    {
        get => _associatedPrograms;
        private set => this.RaiseAndSetIfChanged(ref _associatedPrograms, value);
    }

    /// <summary>
    /// 是否显示关联程序下拉框（OpenFile 模式下有多个关联程序时显示）
    /// </summary>
    public bool ShowAssociatedPrograms => LaunchType == LaunchType.OpenFile && AssociatedPrograms.Count > 0;

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
    /// CMD 工作目录（可选）
    /// </summary>
    private string _cmdWorkingDirectory = string.Empty;
    public string CmdWorkingDirectory
    {
        get => _cmdWorkingDirectory;
        set => this.RaiseAndSetIfChanged(ref _cmdWorkingDirectory, value);
    }

    /// <summary>
    /// CMD 执行后是否保持窗口打开
    /// </summary>
    private bool _cmdKeepWindowOpen;
    public bool CmdKeepWindowOpen
    {
        get => _cmdKeepWindowOpen;
        set => this.RaiseAndSetIfChanged(ref _cmdKeepWindowOpen, value);
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
    /// 浏览工作目录命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> BrowseWorkingDirectoryCommand { get; }

    /// <summary>
    /// 浏览文件夹命令（用于 OpenFolder 模式）
    /// </summary>
    public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }

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
        new LaunchTypeItem(LaunchType.OpenCmd, L.ProjectDialog_LaunchType_OpenCmd),
        new LaunchTypeItem(LaunchType.OpenFolder, L.ProjectDialog_LaunchType_OpenFolder)
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
        BrowseWorkingDirectoryCommand = ReactiveCommand.Create(BrowseWorkingDirectory);
        BrowseFolderCommand = ReactiveCommand.Create(BrowseFolder);
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
                    LaunchType.OpenFolder => string.IsNullOrWhiteSpace(path) ? L.ProjectDialog_Error_EmptyPath : null,
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

        // 启动类型变化时，如果不是 OpenFile 则清空关联程序列表
        this.WhenAnyValue(x => x.LaunchType)
            .Subscribe(lt =>
            {
                if (lt != LaunchType.OpenFile)
                {
                    AssociatedPrograms = Array.Empty<AssociatedProgram>();
                }
                this.RaisePropertyChanged(nameof(ShowAssociatedPrograms));
            })
            .DisposeWith(Disposables);

        // 设置初始图标
        UpdateIcon();
    }

    /// <summary>
    /// 更新图标逻辑
    /// </summary>
    private void UpdateIcon()
    {
        if (_isIconCustomized) return;
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
                LaunchType.OpenFolder => "Images/folder.png",
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
        _isIconCustomized = !string.IsNullOrEmpty(project.CustomIconPath); // ✅ 有自定义图标则标记
        IconPath = project.CustomIconPath;

        // 从项目配置中设置启动类型
        LaunchType = project.LaunchType;
        DefaultProgram = project.DefaultProgram ?? string.Empty;
        LaunchArguments = project.LaunchArguments ?? string.Empty;
        WebUrl = project.WebUrl ?? string.Empty;
        CmdCommand = project.CmdCommand ?? string.Empty;
        CmdWorkingDirectory = project.CmdWorkingDirectory ?? string.Empty;
        CmdKeepWindowOpen = project.CmdKeepWindowOpen;
        RunAsAdmin = project.RunAsAdmin;

        this.RaisePropertyChanged(nameof(DialogTitle));
        this.RaisePropertyChanged(nameof(ConfirmButtonText));
        this.RaisePropertyChanged(nameof(IsEditMode));
        this.RaisePropertyChanged(nameof(IsAddMode));

        _logger.LogInformation(string.Format(L.Log_InitializedEditMode, project.Id));
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
        CmdWorkingDirectory = string.Empty;
        CmdKeepWindowOpen = false;
        RunAsAdmin = false;
        Description = string.Empty;
        IconPath = null;
        ErrorMessage = null;
        HasError = false;
        _isIconCustomized = false;  // ✅ 清除标志
        AssociatedPrograms = Array.Empty<AssociatedProgram>();

    }

    private void BrowsePath()
    {
        var path = _dialogService.ShowOpenFileDialog(
            L.Dialog_SelectProjectPath_Filter,
            L.Dialog_SelectProjectPath_Title);

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

            _logger.LogInformation(string.Format(L.Log_UserSelectedPath, path));
        }
    }

    /// <summary>
    /// 自动检测文件的默认打开方式并填充关联程序列表
    /// </summary>
    private void DetectDefaultProgram(string filePath)
    {
        try
        {
            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            // 获取所有关联程序
            var programs = _fileAssociationService.GetAssociatedPrograms(extension);
            AssociatedPrograms = programs;
            this.RaisePropertyChanged(nameof(ShowAssociatedPrograms));

            // 设置默认程序为第一个关联程序
            var defaultProgram = programs.FirstOrDefault();
            if (defaultProgram != null)
            {
                DefaultProgram = defaultProgram.ExePath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(string.Format(L.Log_DetectDefaultProgramFailed, ex.Message));
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
            L.Dialog_SelectProgram_Filter,
            L.Dialog_SelectProgram_Title);

        if (!string.IsNullOrEmpty(program))
        {
            DefaultProgram = program;
            _logger.LogInformation(string.Format(L.Log_UserSelectedProgram, program));
        }
    }

    private void BrowseIcon()
    {
        var iconPath = _dialogService.ShowOpenFileDialog(
         L.Dialog_SelectIcon_Filter,
         L.Dialog_SelectIcon_Title);

        if (!string.IsNullOrEmpty(iconPath))
        {
            IconPath = iconPath;
            _isIconCustomized = true;  // ✅ 标记为用户自定义
            _logger.LogInformation(string.Format(L.Log_UserSelectedIcon, iconPath));
        }
    }

    private void BrowseWorkingDirectory()
    {
        var directory = _dialogService.ShowSelectFolderDialog(L.Dialog_SelectWorkingDirectory_Title);

        if (!string.IsNullOrEmpty(directory))
        {
            CmdWorkingDirectory = directory;
            _logger.LogInformation(string.Format(L.Log_UserSelectedWorkingDirectory, directory));
        }
    }

    private void BrowseFolder()
    {
        var folder = _dialogService.ShowSelectFolderDialog(L.Dialog_SelectProjectPath_Title);

        if (!string.IsNullOrEmpty(folder))
        {
            ProjectPath = folder;

            // 自动提取项目名称（从文件夹名）
            if (string.IsNullOrWhiteSpace(ProjectName) || Mode == DialogMode.Add)
            {
                ProjectName = System.IO.Path.GetFileName(folder);
            }

            _logger.LogInformation(string.Format(L.Log_UserSelectedPath, folder));
        }
    }

    private void ResetIcon()
    {
        _isIconCustomized = false;  // ✅ 清除自定义标志
        UpdateIcon();
        _logger.LogInformation(L.Log_ResetIconDefault);
    }

    private async Task ConfirmAsync()
    {
        if (HasError || string.IsNullOrWhiteSpace(ProjectName))
        {
            return;
        }
        var f = BuildLaunchFields();  // ✅ 统一构建，不再重复

        if (Mode == DialogMode.Add)
        {
            _logger.LogInformation(string.Format(L.Log_ConfirmAddProject, ProjectName));
            if (_projectAppService == null) { Close(null); return; }

            var dto = new CreateProjectDto
            {
                Name = ProjectName,
                Description = Description,
                CustomIconPath = IconPath,
                LaunchType = LaunchType,
                Path = f.path,
                DefaultProgram = f.defaultProgram,
                LaunchArguments = f.launchArguments,
                WebUrl = f.webUrl,
                CmdCommand = f.cmdCommand,
                CmdWorkingDirectory = f.cmdWorkingDirectory,
                CmdKeepWindowOpen = f.cmdKeepWindowOpen,
                RunAsAdmin = f.runAsAdmin
            };
            Close(await _projectAppService.CreateAsync(dto));
        }
        else
        {
            if (_projectAppService == null || !EditProjectId.HasValue) { Close(null); return; }

            var dto = new UpdateProjectDto
            {
                Id = EditProjectId.Value,
                Name = ProjectName,
                Description = Description,
                CustomIconPath = IconPath,
                LaunchType = LaunchType,
                Path = f.path,
                DefaultProgram = f.defaultProgram,
                LaunchArguments = f.launchArguments,
                WebUrl = f.webUrl,
                CmdCommand = f.cmdCommand,
                CmdWorkingDirectory = f.cmdWorkingDirectory,
                CmdKeepWindowOpen = f.cmdKeepWindowOpen,
                RunAsAdmin = f.runAsAdmin
            };
            Close(await _projectAppService.UpdateAsync(dto));
        }
    }
    // ✅ Bug3 & Bug4修复：提取公共方法，统一处理 Path，消除重复
    private (string path, string? defaultProgram, string? launchArguments,
             string? webUrl, string? cmdCommand, string? cmdWorkingDirectory,
             bool cmdKeepWindowOpen, bool runAsAdmin) BuildLaunchFields()
    {
        var isFileOrExe = LaunchType is LaunchType.OpenFile or LaunchType.OpenExe;
        var isCmd = LaunchType == LaunchType.OpenCmd;
        var isWeb = LaunchType == LaunchType.OpenWebUrl;
        var isFolder = LaunchType == LaunchType.OpenFolder;

        return (
            // ✅ Bug3修复：OpenExe 也保留 ProjectPath（作为传给程序的目标路径）
            // OpenFolder 也需要 Path
            path: (LaunchType == LaunchType.OpenFile || LaunchType == LaunchType.OpenExe || LaunchType == LaunchType.OpenFolder)
                  ? ProjectPath
                  : string.Empty,
            defaultProgram: isFileOrExe && !string.IsNullOrWhiteSpace(DefaultProgram) ? DefaultProgram : null,
            launchArguments: isFileOrExe && !string.IsNullOrWhiteSpace(LaunchArguments) ? LaunchArguments : null,
            webUrl: isWeb && !string.IsNullOrWhiteSpace(WebUrl) ? WebUrl : null,
            cmdCommand: isCmd && !string.IsNullOrWhiteSpace(CmdCommand) ? CmdCommand : null,
            cmdWorkingDirectory: isCmd && !string.IsNullOrWhiteSpace(CmdWorkingDirectory) ? CmdWorkingDirectory : null,
            cmdKeepWindowOpen: isCmd && CmdKeepWindowOpen,
            runAsAdmin: isFileOrExe && RunAsAdmin
        );
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
