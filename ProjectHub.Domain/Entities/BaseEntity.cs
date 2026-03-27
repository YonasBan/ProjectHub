
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
}
