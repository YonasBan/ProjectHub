using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Core.Extensions;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using Splat;
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
public partial class ProjectViewModel : ReactiveObject
{
    private ProjectDto _projectDto;
    private readonly IProjectAppService? _projectAppService;

    public long Id => _projectDto.Id;
    
    public string Name => _projectDto.Name;
    
    public ProjectType Type => _projectDto.Type;
    
    public string Path => _projectDto.Path;
    
    public string? CustomIconPath => _projectDto.CustomIconPath;
    
    public string? Description => _projectDto.Description;
    
    /// <summary>
    /// 关联的工作文件夹 ID 列表
    /// </summary>
    public IReadOnlyList<long> WorkFolderIds => _projectDto.WorkFolderIds;
    
    /// <summary>
    /// 关联的工作空间 ID 列表
    /// </summary>
    public IReadOnlyList<long> WorkSpaceIds => _projectDto.WorkSpaceIds;
    
    public DateTime? LastOpenedAt => _projectDto.LastOpenedAt;
    
    public string LastOpenedDisplay => _projectDto.LastOpenedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
    
    public int LaunchCount => _projectDto.LaunchCount;
    
    /// <summary>
    /// 本地化字符串访问器
    /// </summary>
    private static LocalizedStrings L => Locator.Current.GetService<LocalizedStrings>()!;
    
    /// <summary>
    /// 启动次数显示文本（支持本地化）
    /// </summary>
    public string LaunchCountDisplay => string.Format(L.Project_LaunchCount, LaunchCount);
    
    public string TotalUsageDurationDisplay => _projectDto.TotalUsageDuration.ToHumanReadableString();
    
    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        private set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }
    
    public DateTime? FavoritedAt => _projectDto.FavoritedAt;
    
    public DateTime? Deadline => _projectDto.Deadline;
    
    public long? DiskSpaceBytes => _projectDto.DiskSpaceBytes;
    
    public string DiskSpaceDisplay => _projectDto.DiskSpaceBytes?.FormatFileSize() ?? "Not calculated";
    
    public long? CleanableSpaceBytes => _projectDto.CleanableSpaceBytes;
    
    public string CleanableSpaceDisplay => _projectDto.CleanableSpaceBytes?.FormatFileSize() ?? "Not calculated";

    /// <summary>
    /// Project type display name
    /// </summary>
    public string TypeDisplayName => GetTypeDisplayName(_projectDto.Type);

    /// <summary>
    /// Icon path for display
    /// If CustomIconPath is set, use it; otherwise use project path to extract default program icon
    /// </summary>
    public string IconPath => !string.IsNullOrEmpty(CustomIconPath) ? CustomIconPath : Path;

    /// <summary>
    /// Toggle favorite status command
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; }

    /// <summary>
    /// Edit project command - requests parent to open edit dialog
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; }

    /// <summary>
    /// Delete project command - requests parent to handle deletion with confirmation
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    /// <summary>
    /// Launch project command - starts the project with default program
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchCommand { get; }

    /// <summary>
    /// Send edit request message via MessageBus
    /// </summary>
    private void SendEditRequest() => MessageBus.Current.SendMessage(new ProjectEditRequestMessage(this));

    /// <summary>
    /// Send delete request message via MessageBus
    /// </summary>
    private void SendDeleteRequest() => MessageBus.Current.SendMessage(new ProjectDeleteRequestMessage(this));

    /// <summary>
    /// Send favorite changed message via MessageBus
    /// </summary>
    private void SendFavoriteChanged() => MessageBus.Current.SendMessage(new ProjectFavoriteChangedMessage(this));

    /// <summary>
    /// Send launched message via MessageBus
    /// </summary>
    private void SendLaunched() => MessageBus.Current.SendMessage(new ProjectLaunchedMessage(this));

    public ProjectViewModel(ProjectDto projectDto, IProjectAppService? projectAppService = null)
    {
        _projectDto = projectDto;
        _projectAppService = projectAppService;
        _isFavorite = projectDto.IsFavorite;
        
        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(SendEditRequest);
        DeleteCommand = ReactiveCommand.Create(SendDeleteRequest);
        LaunchCommand = ReactiveCommand.CreateFromTask(LaunchAsync);
        
        // 订阅语言变化，刷新本地化显示属性
        L?.CultureChanged.Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(LaunchCountDisplay));
        });
    }

    /// <summary>
    /// Get the underlying DTO for editing
    /// </summary>
    public ProjectDto GetProjectDto() => _projectDto;

    /// <summary>
    /// Toggle favorite status
    /// </summary>
    private async Task ToggleFavoriteAsync()
    {
        if (_projectAppService == null) return;
        
        var newFavoriteStatus = !IsFavorite;
        await _projectAppService.SetFavoriteAsync(Id, newFavoriteStatus);
        IsFavorite = newFavoriteStatus;
        
        // 通过 MessageBus 通知收藏状态变化
        SendFavoriteChanged();
    }

    /// <summary>
    /// Update the ViewModel after project is edited
    /// </summary>
    public void UpdateFromDto(ProjectDto updatedDto)
    {
        // Note: This is a simplified update
        // In a real scenario, you might want to recreate the ViewModel or use a more sophisticated update mechanism
        _projectDto = updatedDto;
        this.RaisePropertyChanged(string.Empty); // Notify all properties changed
    }

    /// <summary>
    /// Launch the project
    /// </summary>
    private async Task LaunchAsync()
    {
        if (_projectAppService == null) return;
        
        await _projectAppService.LaunchAsync(Id);
        
        // 更新启动次数（假设 LaunchAsync 会更新数据库中的计数）
        // 这里需要重新获取项目信息来更新 LaunchCount
        var updatedProject = await _projectAppService.GetByIdAsync(Id);
        if (updatedProject != null)
        {
            _projectDto = updatedProject;
            this.RaisePropertyChanged(nameof(LaunchCount));
            this.RaisePropertyChanged(nameof(LastOpenedAt));
            
            // 通过 MessageBus 通知项目已启动
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
