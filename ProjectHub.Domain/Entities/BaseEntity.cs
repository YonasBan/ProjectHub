using ProjectHub.Domain.Events;

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
    /// 域事件列表 (用于最终一致性)
    /// </summary>
    private readonly List<BaseDomainEvent> _domainEvents = new();
    
    /// <summary>
    /// 获取并清除所有待处理的域事件
    /// </summary>
    public IReadOnlyCollection<BaseDomainEvent> GetDomainEvents()
    {
        var events = _domainEvents.AsReadOnly();
        _domainEvents.Clear();
        return events;
    }

    /// <summary>
    /// 添加域事件
    /// </summary>
    protected void AddDomainEvent(BaseDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
