namespace ProjectHub.Domain.Entities;

/// <summary>
/// 项目-标签关联实体
/// 实现多对多关系：一个项目可以有多个标签，一个标签可以关联多个项目
/// </summary>
public class ProjectTag : BaseEntity
{
    /// <summary>
    /// 项目 ID
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// 标签 ID
    /// </summary>
    public long TagId { get; private set; }

    /// <summary>
    /// 关联的导航属性 - Project
    /// </summary>
    public Project? Project { get; private set; }

    /// <summary>
    /// 关联的导航属性 - Tag
    /// </summary>
    public Tag? Tag { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private ProjectTag()
    {
    }

    /// <summary>
    /// 工厂方法：创建项目-标签关联
    /// </summary>
    public static ProjectTag Create(long projectId, long tagId)
    {
        if (projectId <= 0)
            throw new ArgumentException("项目 ID 必须大于 0", nameof(projectId));

        if (tagId <= 0)
            throw new ArgumentException("标签 ID 必须大于 0", nameof(tagId));

        var projectTag = new ProjectTag
        {
            ProjectId = projectId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };

        return projectTag;
    }
}
