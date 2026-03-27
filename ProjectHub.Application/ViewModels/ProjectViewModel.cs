using ProjectHub.Application.Interfaces;
using ProjectHub.Core.Extensions;
using ProjectHub.Domain.Entities;
using ReactiveUI;

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
    private readonly ProjectDto _projectDto;
    // TODO: Inject application service for operations
    // private readonly IProjectAppService _projectAppService;

    public long Id => _projectDto.Id;
    
    public string Name => _projectDto.Name;
    
    public ProjectType Type => _projectDto.Type;
    
    public string Path => _projectDto.Path;
    
    public string? CustomIconPath => _projectDto.CustomIconPath;
    
    public string? ColorTag => _projectDto.ColorTag;
    
    public string? Description => _projectDto.Description;
    
    public long? GroupId => _projectDto.GroupId;
    
    public string? GroupName => _projectDto.GroupName;
    
    public DateTime? LastOpenedAt => _projectDto.LastOpenedAt;
    
    public string LastOpenedDisplay => _projectDto.LastOpenedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
    
    public int LaunchCount => _projectDto.LaunchCount;
    
    public string TotalUsageDurationDisplay => _projectDto.TotalUsageDuration.ToHumanReadableString();
    
    public bool IsArchived => _projectDto.IsArchived;
    
    public bool IsFavorite => _projectDto.IsFavorite;
    
    public DateTime? Deadline => _projectDto.Deadline;
    
    public long? DiskSpaceBytes => _projectDto.DiskSpaceBytes;
    
    public string DiskSpaceDisplay => _projectDto.DiskSpaceBytes?.FormatFileSize() ?? "Not calculated";
    
    public long? CleanableSpaceBytes => _projectDto.CleanableSpaceBytes;
    
    public string CleanableSpaceDisplay => _projectDto.CleanableSpaceBytes?.FormatFileSize() ?? "Not calculated";
    
    public IReadOnlyList<TagViewModel> Tags { get; }

    /// <summary>
    /// Project type display name
    /// </summary>
    public string TypeDisplayName => GetTypeDisplayName(_projectDto.Type);

    public ProjectViewModel(ProjectDto projectDto)
    {
        _projectDto = projectDto;
        
        Tags = projectDto.Tags
            .Select(t => new TagViewModel(t))
            .ToList();
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

    /// <summary>
    /// Get icon path for project type (can be customized as needed)
    /// </summary>
    public static string GetTypeIconPath(ProjectType type)
    {
        return type switch
        {
            ProjectType.VisualStudio => "/UI/Assets/Icons/vs_icon.png",
            ProjectType.Android => "/UI/Assets/Icons/android_icon.png",
            ProjectType.Cpp => "/UI/Assets/Icons/cpp_icon.png",
            ProjectType.Tool => "/UI/Assets/Icons/tool_icon.png",
            ProjectType.Document => "/UI/Assets/Icons/doc_icon.png",
            ProjectType.Folder => "/UI/Assets/Icons/folder_icon.png",
            _ => "/UI/Assets/Icons/default_icon.png"
        };
    }
}
