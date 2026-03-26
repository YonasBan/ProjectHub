using ProjectHub.Domain.Events;

namespace ProjectHub.Domain.Entities;

/// <summary>
/// 分组实体
/// 用于对项目进行分类管理 (如：工作、学习、个人等)
/// </summary>
public class Group : BaseEntity
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 分组描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 分组图标路径 (可选)
    /// </summary>
    public string? IconPath { get; private set; }

    /// <summary>
    /// 排序顺序
    /// 数字越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 是否展开状态 (UI 状态，可选存储)
    /// </summary>
    public bool IsExpanded { get; private set; }

    /// <summary>
    /// 关联的项目数量 (计算字段，不存储)
    /// </summary>
    public int ProjectCount { get; private set; }

    // ========== DDD 领域行为 ==========

    public Group()
    {
    }

    /// <summary>
    /// 工厂方法：创建分组
    /// </summary>
    public static Group Create(string name, int sortOrder = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("分组名称不能为空", nameof(name));

        var group = new Group
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            IsExpanded = true
        };

        group.AddDomainEvent(new GroupCreatedDomainEvent(group.Id, group.Name));

        return group;
    }

    /// <summary>
    /// 更新分组信息
    /// </summary>
    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("分组名称不能为空", nameof(name));

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
}
