using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 数据传输对象 (DTOs)
/// 用于应用层和 UI 层之间的数据传递
/// </summary>

/// <summary>
/// 项目 DTO
/// </summary>
public class ProjectDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? CustomIconPath { get; set; }
    public string? ColorTag { get; set; }
    public string? Description { get; set; }
    public long? GroupId { get; set; }
    public string? GroupName { get; set; } // 导航属性
    public DateTime? LastOpenedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int LaunchCount { get; set; }
    public TimeSpan TotalUsageDuration => TimeSpan.FromMilliseconds(TotalUsageDurationMs);
    public long TotalUsageDurationMs { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? Deadline { get; set; }
    public long? CustomerId { get; set; }
    public long? DiskSpaceBytes { get; set; }
    public long? CleanableSpaceBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 关联的标签列表
    /// </summary>
    public IReadOnlyList<TagDto> Tags { get; set; } = new List<TagDto>();
}

/// <summary>
/// 创建项目输入 DTO
/// </summary>
public class CreateProjectDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public ProjectType Type { get; set; }
    
    [Required]
    public string Path { get; set; } = string.Empty;
    
    public long? GroupId { get; set; }
    public string? Description { get; set; }
    public string? CustomIconPath { get; set; }
    public string? ColorTag { get; set; }
    public IEnumerable<long> TagIds { get; set; } = new List<long>();
}

/// <summary>
/// 更新项目输入 DTO
/// </summary>
public class UpdateProjectDto
{
    [Required]
    public long Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public string? ColorTag { get; set; }
    public long? GroupId { get; set; }
    public DateTime? Deadline { get; set; }
    public long? CustomerId { get; set; }
}

/// <summary>
/// 分组 DTO
/// </summary>
public class GroupDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public int SortOrder { get; set; }
    public bool IsExpanded { get; set; }
    public int ProjectCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建分组输入 DTO
/// </summary>
public class CreateGroupDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新分组输入 DTO
/// </summary>
public class UpdateGroupDto
{
    [Required]
    public long Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}

/// <summary>
/// 标签 DTO
/// </summary>
public class TagDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int ProjectCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建标签输入 DTO
/// </summary>
public class CreateTagDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Color { get; set; }
}

/// <summary>
/// 更新标签输入 DTO
/// </summary>
public class UpdateTagDto
{
    [Required]
    public long Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Color { get; set; }
}

/// <summary>
/// 清理扫描结果 DTO
/// </summary>
public class CleanupScanResultDto
{
    public long TotalSpaceBytes { get; set; }
    public long CleanableSpaceBytes { get; set; }
    public string TotalSpaceDisplay => FormatSize(TotalSpaceBytes);
    public string CleanableSpaceDisplay => FormatSize(CleanableSpaceBytes);
    public IReadOnlyList<CleanupItemInfoDto> Items { get; set; } = new List<CleanupItemInfoDto>();

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// 清理项信息 DTO
/// </summary>
public class CleanupItemInfoDto
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeDisplay => $"{FormatSize(SizeBytes)}";
    public CleanupCategory Category { get; set; }
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// 清理预设方案 DTO
/// </summary>
public class CleanupProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CleanupCategory Category { get; set; }
    public IEnumerable<string> IncludedPatterns { get; set; } = new List<string>();
}

/// <summary>
/// 项目包 DTO
/// </summary>
public class ProjectBundleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public string? ColorTag { get; set; }
    public bool UseCustomOrder { get; set; }
    public int DefaultIntervalSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// 项目包成员列表
    /// </summary>
    public IReadOnlyList<ProjectBundleItemDto> Items { get; set; } = new List<ProjectBundleItemDto>();
    
    /// <summary>
    /// 成员项目详情 (可选加载)
    /// </summary>
    public IReadOnlyList<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
}

/// <summary>
/// 项目包成员 DTO
/// </summary>
public class ProjectBundleItemDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? IntervalSeconds { get; set; }
}

/// <summary>
/// 创建项目包输入 DTO
/// </summary>
public class CreateProjectBundleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public IEnumerable<ProjectBundleItemInput> Items { get; set; } = new List<ProjectBundleItemInput>();
}

/// <summary>
/// 项目包成员输入
/// </summary>
public class ProjectBundleItemInput
{
    public long ProjectId { get; set; }
    public int Order { get; set; }
    public int? IntervalSeconds { get; set; }
}

/// <summary>
/// 更新项目包输入 DTO
/// </summary>
public class UpdateProjectBundleDto
{
    [Required]
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public string? ColorTag { get; set; }
    public bool? UseCustomOrder { get; set; }
    public int? DefaultIntervalSeconds { get; set; }
    public IEnumerable<ProjectBundleItemInput>? Items { get; set; }
}
