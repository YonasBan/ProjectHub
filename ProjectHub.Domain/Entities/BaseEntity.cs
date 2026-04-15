
namespace ProjectHub.Domain.Entities;

/// <summary>
/// 领域实体基类
/// 所有实体必须继承此类，提供唯一标识和审计字段
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 实体唯一标识 (主键)
    /// </summary>
    public long Id { get; protected set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// 是否已删除（逻辑删除标记）
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// 逻辑删除实体
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 恢复已删除的实体
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
