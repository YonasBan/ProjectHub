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
    /// Event raised when edit is requested
    /// </summary>
    public event EventHandler<ProjectViewModel>? EditRequested;

    public ProjectViewModel(ProjectDto projectDto, IProjectAppService? projectAppService = null)
    {
        _projectDto = projectDto;
        _projectAppService = projectAppService;
        _isFavorite = projectDto.IsFavorite;
        
        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(RequestEdit);
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
    }

    /// <summary>
    /// Request edit - raises event for parent ViewModel to handle
    /// </summary>
    private void RequestEdit()
    {
        EditRequested?.Invoke(this, this);
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
