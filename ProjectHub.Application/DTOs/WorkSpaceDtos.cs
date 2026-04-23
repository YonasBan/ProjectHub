using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.DTOs;

/// <summary>
/// 工作空间 DTO
/// </summary>
public class WorkSpaceDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public int SortOrder { get; set; }
    public int ProjectCount { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? FavoritedAt { get; set; }
    public DateTime? LastOpenedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 关联的文件夹ID列表
    /// </summary>
    public IReadOnlyList<long> WorkFolderIds { get; set; } = [];

    /// <summary>
    /// 是否使用自定义启动顺序
    /// </summary>
    public bool UseCustomLaunchOrder { get; set; }

    /// <summary>
    /// 默认启动间隔时间 (秒)
    /// </summary>
    public int DefaultLaunchIntervalSeconds { get; set; }

    /// <summary>
    /// 项目启动顺序配置
    /// </summary>
    public IReadOnlyList<WorkSpaceProjectLaunchOrderDto> LaunchOrders { get; set; } = [];

    /// <summary>
    /// 关联的标签列表 (可选，用于详情显示)
    /// </summary>
    public IReadOnlyList<TagDto> Tags { get; set; } = [];
}

/// <summary>
/// 创建工作空间输入 DTO
/// </summary>
public class CreateWorkSpaceDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsFavorite { get; set; }
}

/// <summary>
/// 更新工作空间输入 DTO
/// </summary>
public class UpdateWorkSpaceDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool? IsFavorite { get; set; }
}

/// <summary>
/// 工作空间项目设置 DTO - 用于弹窗设置
/// </summary>
public class WorkSpaceProjectSettingsDto
{
    public long WorkSpaceId { get; set; }
    public string WorkSpaceName { get; set; } = string.Empty;

    /// <summary>
    /// 工作空间包含的所有项目
    /// </summary>
    public IReadOnlyList<ProjectDto> AllProjects { get; set; } = [];

    /// <summary>
    /// 项目设置列表（包含启用状态）
    /// </summary>
    public IReadOnlyList<WorkSpaceProjectSettingItemDto> ProjectSettings { get; set; } = [];

    /// <summary>
    /// 关联的标签列表 (可选，用于详情显示)
    /// </summary>
    public IReadOnlyList<TagDto> Tags { get; set; } = [];
}

/// <summary>
/// 工作空间项目设置项 DTO
/// </summary>
public class WorkSpaceProjectSettingItemDto
{
    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新工作空间项目设置输入 DTO
/// </summary>
public class UpdateWorkSpaceProjectSettingsDto
{
    [Required]
    public long WorkSpaceId { get; set; }

    /// <summary>
    /// 是否使用自定义启动顺序
    /// </summary>
    public bool? UseCustomLaunchOrder { get; set; }

    /// <summary>
    /// 默认启动间隔时间 (秒)
    /// </summary>
    public int? DefaultLaunchIntervalSeconds { get; set; }

    /// <summary>
    /// 项目启动顺序配置 (可选)
    /// </summary>
    public IEnumerable<WorkSpaceProjectLaunchOrderDto>? LaunchOrders { get; set; }
}

/// <summary>
/// 工作空间项目启动顺序配置 DTO
/// </summary>
public class WorkSpaceProjectLaunchOrderDto
{
    public long ProjectId { get; set; }
    public int Order { get; set; }
    public int? IntervalSeconds { get; set; }
}
