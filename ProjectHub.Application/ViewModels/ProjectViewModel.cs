using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Core.Extensions;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// Project ViewModel (for UI presentation)
/// 
/// DDD Design Points:
/// - This is a Presentation Model, not a Domain Model
/// - Contains UI-specific formatting logic
/// - Interacts with domain layer through application services
/// </summary>
public partial class ProjectViewModel : ItemViewModelBase<ProjectDto>
{
    private readonly IProjectAppService? _projectAppService;

    public ProjectType Type => ((ProjectDto)_dto).Type;

    public string Path => ((ProjectDto)_dto).Path;

    public string? CustomIconPath => ((ProjectDto)_dto).CustomIconPath;

    public string LastOpenedDisplay => ((ProjectDto)_dto).LastOpenedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";

    public int LaunchCount => ((ProjectDto)_dto).LaunchCount;

    /// <summary>
    /// 启动次数显示文本（支持本地化）
    /// </summary>
    public string LaunchCountDisplay => string.Format(L.Project_LaunchCount, LaunchCount);

    public string TotalUsageDurationDisplay => ((ProjectDto)_dto).TotalUsageDuration.ToHumanReadableString();

    public DateTime? Deadline => ((ProjectDto)_dto).Deadline;

    public long? DiskSpaceBytes => ((ProjectDto)_dto).DiskSpaceBytes;

    public string DiskSpaceDisplay => ((ProjectDto)_dto).DiskSpaceBytes?.FormatFileSize() ?? "Not calculated";

    public long? CleanableSpaceBytes => ((ProjectDto)_dto).CleanableSpaceBytes;

    public string CleanableSpaceDisplay => ((ProjectDto)_dto).CleanableSpaceBytes?.FormatFileSize() ?? "Not calculated";

    /// <summary>
    /// Project type display name
    /// </summary>
    public string TypeDisplayName => GetTypeDisplayName(((ProjectDto)_dto).Type);

    /// <summary>
    /// Icon path for display
    /// If CustomIconPath is set, use it; otherwise use project path to extract default program icon
    /// </summary>
    public string IconPath => !string.IsNullOrEmpty(CustomIconPath) ? CustomIconPath : Path;

    /// <summary>
    /// Launch project command - starts the project with default program
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchCommand { get; }

    public ProjectViewModel(ProjectDto projectDto, IProjectAppService? projectAppService = null)
        : base(projectDto)
    {
        _projectAppService = projectAppService;
        _isFavorite = projectDto.IsFavorite;

        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(SendEditRequest);
        DeleteCommand = ReactiveCommand.Create(SendDeleteRequest);
        LaunchCommand = ReactiveCommand.CreateFromTask(LaunchAsync);
        MoveToFolderCommand = ReactiveCommand.Create(SendMoveToFolderRequest);

        // 订阅语言变化，刷新本地化显示属性
        L?.CultureChanged.Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(LaunchCountDisplay));
        });
    }

    #region Base Class Implementations

    protected override long GetId() => ((ProjectDto)_dto).Id;

    protected override string GetName() => ((ProjectDto)_dto).Name;

    protected override string? GetDescription() => ((ProjectDto)_dto).Description;

    protected override DateTime? GetFavoritedAt() => ((ProjectDto)_dto).FavoritedAt;

    protected override DateTime? GetLastOpenedAt() => ((ProjectDto)_dto).LastOpenedAt;

    protected override void SendEditRequest() => MessageBus.Current.SendMessage(new ProjectEditRequestMessage(this));

    protected override void SendDeleteRequest() => MessageBus.Current.SendMessage(new ProjectDeleteRequestMessage(this));

    protected override void SendFavoriteChanged() => MessageBus.Current.SendMessage(new ProjectFavoriteChangedMessage(this));

    protected override void SendMoveToFolderRequest() => MessageBus.Current.SendMessage(new ProjectMoveToFolderRequestMessage(this));

    public override ProjectDto GetDto() => (ProjectDto)_dto;

    public override void UpdateFromDto(ProjectDto updatedDto)
    {
        _dto = updatedDto;
        this.RaisePropertyChanged(string.Empty);
    }

    protected override async Task ToggleFavoriteAsync()
    {
        if (_projectAppService == null) return;

        var newFavoriteStatus = !IsFavorite;
        await _projectAppService.SetFavoriteAsync(Id, newFavoriteStatus);
        IsFavorite = newFavoriteStatus;

        SendFavoriteChanged();
    }

    #endregion

    /// <summary>
    /// Send launched message via MessageBus
    /// </summary>
    private void SendLaunched() => MessageBus.Current.SendMessage(new ProjectLaunchedMessage(this));

    /// <summary>
    /// Launch the project
    /// </summary>
    private async Task LaunchAsync()
    {
        if (_projectAppService == null) return;

        await _projectAppService.LaunchAsync(Id);

        var updatedProject = await _projectAppService.GetByIdAsync(Id);
        if (updatedProject != null)
        {
            _dto = updatedProject;
            this.RaisePropertyChanged(nameof(LaunchCount));
            this.RaisePropertyChanged(nameof(LastOpenedAt));
            SendLaunched();
        }
    }

    /// <summary>
    /// Get display name for project type
    /// </summary>
    private static string GetTypeDisplayName(ProjectType type)
    {
        return type switch
        {
            ProjectType.VisualStudio => "Visual Studio",
            ProjectType.Android => "Android",
            ProjectType.Cpp => "C++",
            ProjectType.Tool => "Tool",
            ProjectType.Document => "Document",
            ProjectType.Folder => "Folder",
            _ => "Unknown"
        };
    }
}

// ========== MessageBus Messages ==========

/// <summary>
/// 项目编辑请求消息
/// </summary>
public record ProjectEditRequestMessage(ProjectViewModel Project);

/// <summary>
/// 项目删除请求消息
/// </summary>
public record ProjectDeleteRequestMessage(ProjectViewModel Project);

/// <summary>
/// 项目收藏状态变化消息
/// </summary>
public record ProjectFavoriteChangedMessage(ProjectViewModel Project);

/// <summary>
/// 项目启动消息
/// </summary>
public record ProjectLaunchedMessage(ProjectViewModel Project);

/// <summary>
/// 项目移动到文件夹请求消息
/// </summary>
public record ProjectMoveToFolderRequestMessage(ProjectViewModel Project);
