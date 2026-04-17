using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.DTOs;

/// <summary>
/// 工作文件夹 DTO
/// </summary>
public class WorkFolderDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public int SortOrder { get; set; }
    public bool IsExpanded { get; set; }
    public int ProjectCount { get; set; }
    public int WorkSpaceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 创建工作文件夹输入 DTO
/// </summary>
public class CreateWorkFolderDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public long? ParentId { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>
/// 更新工作文件夹输入 DTO
/// </summary>
public class UpdateWorkFolderDto
{
    [Required]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
