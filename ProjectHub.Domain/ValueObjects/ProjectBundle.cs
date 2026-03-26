namespace ProjectHub.Domain.ValueObjects;

/// <summary>
/// 项目包 (Project Bundle)
/// 将多个项目/工具/文档组合为一个包，一键启动整套环境
/// 
/// DDD 设计：这是一个聚合根，包含多个子项
/// </summary>
public class ProjectBundle
{
    /// <summary>
    /// 项目包唯一标识
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 项目包名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 项目包描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 项目包图标路径 (可选)
    /// </summary>
    public string? IconPath { get; private set; }

    /// <summary>
    /// 颜色标签 (可选)
    /// </summary>
    public string? ColorTag { get; private set; }

    /// <summary>
    /// 是否启用自定义启动顺序
    /// </summary>
    public bool UseCustomOrder { get; private set; }

    /// <summary>
    /// 默认间隔时间 (秒)，0 表示无间隔
    /// </summary>
    public int DefaultIntervalSeconds { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// 项目包成员列表 (有序)
    /// </summary>
    private readonly List<ProjectBundleItem> _items = new();
    
    /// <summary>
    /// 只读的成员列表
    /// </summary>
    public IReadOnlyList<ProjectBundleItem> Items => _items.AsReadOnly();

    // ========== DDD 领域行为 ==========

    private ProjectBundle()
    {
    }

    /// <summary>
    /// 工厂方法：创建项目包
    /// </summary>
    public static ProjectBundle Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("项目包名称不能为空", nameof(name));

        var bundle = new ProjectBundle
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            UseCustomOrder = false,
            DefaultIntervalSeconds = 0,
            CreatedAt = DateTime.UtcNow
        };

        return bundle;
    }

    /// <summary>
    /// 添加项目包成员
    /// </summary>
    /// <param name="projectId">项目 ID</param>
    /// <param name="order">启动顺序 (1-based)</param>
    /// <param name="customIntervalSeconds">自定义间隔时间 (秒)，null 则使用默认值</param>
    public void AddItem(long projectId, int order, int? customIntervalSeconds = null)
    {
        var item = new ProjectBundleItem(
            Id, 
            projectId, 
            order, 
            customIntervalSeconds
        );
        
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 移除项目包成员
    /// </summary>
    public void RemoveItem(long projectId)
    {
        var item = _items.FirstOrDefault(i => i.ProjectId == projectId);
        if (item != null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 更新项目包配置
    /// </summary>
    public void UpdateConfig(bool useCustomOrder, int defaultIntervalSeconds)
    {
        UseCustomOrder = useCustomOrder;
        DefaultIntervalSeconds = defaultIntervalSeconds;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// 项目包成员项
/// </summary>
public class ProjectBundleItem
{
    /// <summary>
    /// 所属项目包 ID
    /// </summary>
    public Guid BundleId { get; private set; }

    /// <summary>
    /// 项目 ID
    /// </summary>
    public long ProjectId { get; private set; }

    /// <summary>
    /// 启动顺序 (1-based)
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// 与下一个项目的间隔时间 (秒)，null 表示使用默认值
    /// </summary>
    public int? IntervalSeconds { get; private set; }

    /// <summary>
    /// 私有构造函数 - 由 ProjectBundle 管理创建
    /// </summary>
    internal ProjectBundleItem(Guid bundleId, long projectId, int order, int? intervalSeconds)
    {
        BundleId = bundleId;
        ProjectId = projectId;
        Order = order;
        IntervalSeconds = intervalSeconds;
    }
}
