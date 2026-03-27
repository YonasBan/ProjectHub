
namespace ProjectHub.Domain.Entities;

/// <summary>
/// WorkSpace 和 Tag 的多对多关联表
/// 一个 WorkSpace 可以有多个标签
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

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private WorkSpaceTag()
    {
    }

    /// <summary>
    /// 工厂方法：创建关联
    /// </summary>
    public static WorkSpaceTag Create(long workSpaceId, long tagId)
    {
        var association = new WorkSpaceTag
        {
            WorkSpaceId = workSpaceId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };
        return association;
    }
}
