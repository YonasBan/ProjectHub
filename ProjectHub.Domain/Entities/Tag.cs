using ProjectHub.Domain.Events;

namespace ProjectHub.Domain.Entities;

/// <summary>
/// 标签实体
/// 支持为项目添加多个标签，实现多维度分类
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 标签颜色 (可选)
    /// 存储 ARGB 值或预定义颜色名
    /// </summary>
    public string? Color { get; private set; }

    /// <summary>
    /// 关联的项目数量 (计算字段)
    /// </summary>
    public int ProjectCount { get; private set; }

    // ========== DDD 领域行为 ==========

    public Tag()
    {
    }

    /// <summary>
    /// 工厂方法：创建标签
    /// </summary>
    public static Tag Create(string name, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("标签名称不能为空", nameof(name));

        var tag = new Tag
        {
            Name = name,
            Color = color,
            CreatedAt = DateTime.UtcNow,
            ProjectCount = 0
        };

        tag.AddDomainEvent(new TagCreatedDomainEvent(tag.Id, tag.Name));

        return tag;
    }

    /// <summary>
    /// 更新标签信息
    /// </summary>
    public void UpdateInfo(string name, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("标签名称不能为空", nameof(name));

        Name = name;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新项目数量统计
    /// </summary>
    public void UpdateProjectCount(int count)
    {
        ProjectCount = count;
    }
}
