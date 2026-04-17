
namespace ProjectHub.Domain.Entities;

/// <summary>
/// 工作文件夹实体
/// 用于对项目进行分类管理 (如：工作、学习、个人等)
/// 一个 WorkFolder 可以包含多个 Project，一个 Project 只能属于一个 WorkFolder
/// </summary>
public class WorkFolder : BaseEntity
{
    /// <summary>
    /// 工作文件夹名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 工作文件夹描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 工作文件夹图标路径 (可选)
    /// </summary>
    public string? IconPath { get; private set; }

    /// <summary>
    /// 排序顺序
    /// 数字越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 父级工作文件夹 ID (可选，用于实现层级结构)
    /// 为空表示根级工作文件夹
    /// </summary>
    public long? ParentId { get; private set; }

    /// <summary>
    /// 是否展开状态 (UI 状态，可选存储)
    /// </summary>
    public bool IsExpanded { get; private set; }

    /// <summary>
    /// 关联的项目数量 (计算字段，不存储)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int ProjectCount { get; private set; }

    /// <summary>
    /// 关联的工作空间数量 (计算字段，不存储)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int WorkSpaceCount { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private WorkFolder()
    {
    }

    /// <summary>
    /// 工厂方法：创建工作文件夹
    /// </summary>
    public static WorkFolder Create(string name, int sortOrder = 0, string? description = null, long? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("工作文件夹名称不能为空", nameof(name));

        var workFolder = new WorkFolder
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow,
            IsExpanded = true
        };
        return workFolder;
    }

    /// <summary>
    /// 更新工作文件夹信息
    /// </summary>
    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("工作文件夹名称不能为空", nameof(name));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置展开/折叠状态
    /// </summary>
    public void SetExpanded(bool isExpanded)
    {
        IsExpanded = isExpanded;
    }

    /// <summary>
    /// 更新项目数量统计
    /// </summary>
    public void UpdateProjectCount(int count)
    {
        ProjectCount = count;
    }

    /// <summary>
    /// 更新工作空间数量统计
    /// </summary>
    public void UpdateWorkSpaceCount(int count)
    {
        WorkSpaceCount = count;
    }
}
