using ProjectHub.Domain.Entities;

namespace ProjectHub.Domain.Events;

/// <summary>
/// 域事件基类
/// 所有域事件必须继承此类
/// </summary>
public abstract class BaseDomainEvent
{
    /// <summary>
    /// 事件唯一标识
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// 项目创建事件
/// </summary>
public class ProjectCreatedDomainEvent : BaseDomainEvent
{
    public long ProjectId { get; }
    public string ProjectName { get; }
    public ProjectType ProjectType { get; }

    public ProjectCreatedDomainEvent(long projectId, string projectName, ProjectType projectType)
    {
        ProjectId = projectId;
        ProjectName = projectName;
        ProjectType = projectType;
    }
}

/// <summary>
/// 项目归档事件
/// </summary>
public class ProjectArchivedDomainEvent : BaseDomainEvent
{
    public long ProjectId { get; }
    public string ProjectName { get; }

    public ProjectArchivedDomainEvent(long projectId, string projectName)
    {
        ProjectId = projectId;
        ProjectName = projectName;
    }
}

/// <summary>
/// 项目恢复事件
/// </summary>
public class ProjectRestoredDomainEvent : BaseDomainEvent
{
    public long ProjectId { get; }
    public string ProjectName { get; }

    public ProjectRestoredDomainEvent(long projectId, string projectName)
    {
        ProjectId = projectId;
        ProjectName = projectName;
    }
}

/// <summary>
/// 分组创建事件
/// </summary>
public class GroupCreatedDomainEvent : BaseDomainEvent
{
    public long GroupId { get; }
    public string GroupName { get; }

    public GroupCreatedDomainEvent(long groupId, string groupName)
    {
        GroupId = groupId;
        GroupName = groupName;
    }
}

/// <summary>
/// 标签创建事件
/// </summary>
public class TagCreatedDomainEvent : BaseDomainEvent
{
    public long TagId { get; }
    public string TagName { get; }

    public TagCreatedDomainEvent(long tagId, string tagName)
    {
        TagId = tagId;
        TagName = tagName;
    }
}
