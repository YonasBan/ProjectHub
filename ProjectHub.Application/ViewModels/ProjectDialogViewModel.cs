using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels;

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

        var canConfirm = this.WhenAnyValue(
            x => x.ProjectName,
            x => x.ProjectPath,
            x => x.DefaultProgram,
            (name, path, program) =>
                !string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(path) &&
                !string.IsNullOrWhiteSpace(program));

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

        // TODO: DefaultProgram 需要从项目配置中获取
        // 暂时使用项目路径作为默认程序
        DefaultProgram = project.Path;

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
        DefaultProgram = string.Empty;
        Description = string.Empty;
        IconPath = null;
        ErrorMessage = null;
        HasError = false;
    }

    private void ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ErrorMessage = L.ProjectDialog_Error_EmptyName;
            HasError = true;
        }
        else if (string.IsNullOrWhiteSpace(ProjectPath))
        {
            ErrorMessage = L.ProjectDialog_Error_EmptyPath;
            HasError = true;
        }
        else if (string.IsNullOrWhiteSpace(DefaultProgram))
        {
            ErrorMessage = L.ProjectDialog_Error_EmptyProgram;
            HasError = true;
        }
        else
        {
            ErrorMessage = null;
            HasError = false;
        }
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
    /// 检测 Visual Studio 路径
    /// </summary>
    private string? DetectVisualStudio()
    {
        // 尝试常见路径
        var vsPaths = new[]
        {
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
        };

        foreach (var path in vsPaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    /// <summary>
    /// 检测 Gradle 路径
    /// </summary>
    private string? DetectGradle()
    {
        // 尝试常见路径
        var gradlePaths = new[]
        {
            @"C:\Program Files\Gradle\bin\gradle.bat",
            @"C:\Gradle\bin\gradle.bat"
        };

        foreach (var path in gradlePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return "gradle";
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
        IconPath = null;
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

            // 创建 CreateProjectDto
            var dto = new CreateProjectDto
            {
                Name = ProjectName,
                Path = ProjectPath,
                Description = Description,
                CustomIconPath = IconPath,
                WorkFolderIds = [],
                WorkSpaceIds = []
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
                var updateDto = new UpdateProjectDto
                {
                    Id = EditProjectId.Value,
                    Name = ProjectName,
                    Description = Description,
                    DefaultProgram = DefaultProgram,
                    WorkFolderIds = [],
                    WorkSpaceIds = []
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
