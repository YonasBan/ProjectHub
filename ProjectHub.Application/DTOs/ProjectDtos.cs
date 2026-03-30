using ProjectHub.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.DTOs;

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
    public string? Description { get; set; }

    /// <summary>
    /// 关联的工作文件夹 ID 列表
    /// </summary>
    public IReadOnlyList<long> WorkFolderIds { get; set; } = [];

    /// <summary>
    /// 关联的工作空间 ID 列表
    /// </summary>
    public IReadOnlyList<long> WorkSpaceIds { get; set; } = [];

    public DateTime? LastOpenedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int LaunchCount { get; set; }
    public TimeSpan TotalUsageDuration => TimeSpan.FromMilliseconds(TotalUsageDurationMs);
    public long TotalUsageDurationMs { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? FavoritedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public long? CustomerId { get; set; }
    public long? DiskSpaceBytes { get; set; }
    public long? CleanableSpaceBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 关联的标签列表 (可选，用于详情显示)
    /// </summary>
    public IReadOnlyList<TagDto> Tags { get; set; } = [];
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

    public string? Description { get; set; }
    public string? CustomIconPath { get; set; }
    public IEnumerable<long> WorkFolderIds { get; set; } = [];
    public IEnumerable<long> WorkSpaceIds { get; set; } = [];
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
    public IEnumerable<long> WorkFolderIds { get; set; } = [];
    public IEnumerable<long> WorkSpaceIds { get; set; } = [];
    public DateTime? Deadline { get; set; }
    public long? CustomerId { get; set; }
}
