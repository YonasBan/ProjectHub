
namespace ProjectHub.Domain.Entities;

/// <summary>
/// Project 和 WorkFolder 的多对多关联表
/// 一个 Project 可以属于多个 WorkFolder
/// </summary>
public class ProjectWorkFolder : BaseEntity
{
    /// <summary>
    /// 项目 ID
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// 工作文件夹 ID
    /// </summary>
    public long WorkFolderId { get; private set; }

    /// <summary>
    /// 排序顺序（在 WorkFolder 中的显示顺序）
    /// </summary>
    public int SortOrder { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private ProjectWorkFolder()
    {
    }

    /// <summary>
    /// 工厂方法：创建关联
    /// </summary>
    public static ProjectWorkFolder Create(long projectId, long workFolderId, int sortOrder = 0)
    {
        var association = new ProjectWorkFolder
        {
            ProjectId = projectId,
            WorkFolderId = workFolderId,
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
