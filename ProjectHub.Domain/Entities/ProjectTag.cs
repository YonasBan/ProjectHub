namespace ProjectHub.Domain.Entities;

/// <summary>
/// 项目 - 标签关联实体 (多对多关系)
/// 一个项目可以有多个标签，一个标签可以应用于多个项目
/// </summary>
public class ProjectTag : BaseEntity
{
    /// <summary>
    /// 项目 ID (外键)
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// 标签 ID (外键)
    /// </summary>
    public long TagId { get; private set; }

    // ========== DDD 领域行为 ==========

    private ProjectTag()
    {
    }

    /// <summary>
    /// 工厂方法：创建项目 - 标签关联
    /// </summary>
    public static ProjectTag Create(long projectId, long tagId)
    {
        var projectTag = new ProjectTag
        {
            ProjectId = projectId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };

        return projectTag;
    }
}
