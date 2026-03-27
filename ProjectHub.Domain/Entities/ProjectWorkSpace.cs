
namespace ProjectHub.Domain.Entities;

/// <summary>
/// Project 和 WorkSpace 的多对多关联表
/// 一个 Project 可以属于多个 WorkSpace
/// </summary>
public class ProjectWorkSpace : BaseEntity
{
    /// <summary>
    /// 项目 ID
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// 工作空间 ID
    /// </summary>
    public long WorkSpaceId { get; private set; }

    /// <summary>
    /// 排序顺序（在 WorkSpace 中的显示顺序）
    /// </summary>
    public int SortOrder { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private ProjectWorkSpace()
    {
    }

    /// <summary>
    /// 工厂方法：创建关联
    /// </summary>
    public static ProjectWorkSpace Create(long projectId, long workSpaceId, int sortOrder = 0)
    {
        var association = new ProjectWorkSpace
        {
            ProjectId = projectId,
            WorkSpaceId = workSpaceId,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
        return association;
    }

    /// <summary>
    /// 更新排序顺序
    /// </summary>
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
