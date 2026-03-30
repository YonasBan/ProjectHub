namespace ProjectHub.Domain.Entities;

/// <summary>
/// 工作空间-标签关联实体
/// 实现多对多关系：一个工作空间可以有多个标签，一个标签可以关联多个工作空间
/// </summary>
public class WorkSpaceTag : BaseEntity
{
    /// <summary>
    /// 工作空间 ID
    /// </summary>
    public long WorkSpaceId { get; private set; }

    /// <summary>
    /// 标签 ID
    /// </summary>
    public long TagId { get; private set; }

    /// <summary>
    /// 关联的导航属性 - WorkSpace
    /// </summary>
    public WorkSpace? WorkSpace { get; private set; }

    /// <summary>
    /// 关联的导航属性 - Tag
    /// </summary>
    public Tag? Tag { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private WorkSpaceTag()
    {
    }

    /// <summary>
    /// 工厂方法：创建工作空间-标签关联
    /// </summary>
    public static WorkSpaceTag Create(long workSpaceId, long tagId)
    {
        if (workSpaceId <= 0)
            throw new ArgumentException("工作空间 ID 必须大于 0", nameof(workSpaceId));

        if (tagId <= 0)
            throw new ArgumentException("标签 ID 必须大于 0", nameof(tagId));

        var workSpaceTag = new WorkSpaceTag
        {
            WorkSpaceId = workSpaceId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };

        return workSpaceTag;
    }
}
