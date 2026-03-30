using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.DTOs;

/// <summary>
/// 标签 DTO
/// </summary>
public class TagDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 关联的项目数量 (可选，用于列表显示)
    /// </summary>
    public int ProjectCount { get; set; }

    /// <summary>
    /// 关联的工作空间数量 (可选，用于列表显示)
    /// </summary>
    public int WorkSpaceCount { get; set; }
}

/// <summary>
/// 创建标签输入 DTO
/// </summary>
public class CreateTagDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>
/// 更新标签输入 DTO
/// </summary>
public class UpdateTagDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Color { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? SortOrder { get; set; }
}

/// <summary>
/// 为项目添加标签输入 DTO
/// </summary>
public class AddProjectTagDto
{
    [Required]
    public long ProjectId { get; set; }

    [Required]
    public long TagId { get; set; }
}

/// <summary>
/// 为工作空间添加标签输入 DTO
/// </summary>
public class AddWorkSpaceTagDto
{
    [Required]
    public long WorkSpaceId { get; set; }

    [Required]
    public long TagId { get; set; }
}
