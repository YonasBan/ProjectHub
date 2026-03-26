namespace ProjectHub.Core.Exceptions;

/// <summary>
/// 领域异常基类
/// 所有业务逻辑异常应继承此类
/// </summary>
public class DomainException : Exception
{
    public DomainException() : base() { }
    
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// 实体未找到异常
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} (ID={entityId}) 未找到")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("验证失败")
    {
        Errors = errors;
    }
}

/// <summary>
/// 业务规则异常
/// </summary>
public class BusinessException : DomainException
{
    public string ErrorCode { get; }

    public BusinessException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
