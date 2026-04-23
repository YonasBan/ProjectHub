
namespace ProjectHub.Domain.Entities;

/// <summary>
/// 工作空间实体
/// 用于关联多个 Project，形成一套工作环境
/// 例如：一个 Android 开发工作空间可能包含 Android 项目、后端 API 项目、文档等
/// </summary>
public class WorkSpace : BaseEntity
{
    /// <summary>
    /// 工作空间名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 工作空间描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 自定义图标路径 (可选)
    /// </summary>
    public string? IconPath { get; private set; }

    /// <summary>
    /// 排序顺序
    /// 数字越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 关联的项目数量 (计算字段，不存储)
    /// </summary>
    public int ProjectCount { get; private set; }

    /// <summary>
    /// 是否已收藏/置顶
    /// </summary>
    public bool IsFavorite { get; private set; }

    /// <summary>
    /// 收藏/置顶时间
    /// 用于排序，数字越大越靠前
    /// </summary>
    public DateTime? FavoritedAt { get; private set; }

    /// <summary>
    /// 最近打开时间
    /// 用于"最近使用"排序，记录用户最后访问该工作空间的时间
    /// </summary>
    public DateTime? LastOpenedAt { get; private set; }

    /// <summary>
    /// 是否使用自定义启动顺序
    /// false: 按照添加顺序启动
    /// true: 按照 WorkSpaceProjectLaunchOrder 中定义的顺序启动
    /// </summary>
    public bool UseCustomLaunchOrder { get; private set; }

    /// <summary>
    /// 默认项目启动间隔时间 (秒)
    /// 默认为 0，表示连续启动不等待
    /// </summary>
    public int DefaultLaunchIntervalSeconds { get; private set; }

    // ========== 导航属性 ==========

    /// <summary>
    /// 关联的标签
    /// </summary>
    public ICollection<WorkSpaceTag> WorkSpaceTags { get; private set; } = new List<WorkSpaceTag>();

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private WorkSpace()
    {
    }

    /// <summary>
    /// 工厂方法：创建工作空间
    /// </summary>
    public static WorkSpace Create(string name, int sortOrder = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("工作空间名称不能为空", nameof(name));

        var workSpace = new WorkSpace
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            IsFavorite = false,
            UseCustomLaunchOrder = false,
            DefaultLaunchIntervalSeconds = 0
        };
        return workSpace;
    }

    /// <summary>
    /// 更新工作空间信息
    /// </summary>
    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("工作空间名称不能为空", nameof(name));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新项目数量统计
    /// </summary>
    public void UpdateProjectCount(int count)
    {
        ProjectCount = count;
    }

    /// <summary>
    /// 设置收藏状态
    /// </summary>
    public void SetFavorite(bool isFavorite)
    {
        if (IsFavorite == isFavorite) return;
        
        IsFavorite = isFavorite;
        FavoritedAt = isFavorite ? DateTime.UtcNow : null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 记录工作空间被打开
    /// 更新最近打开时间
    /// </summary>
    public void RecordOpen()
    {
        LastOpenedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新启动配置
    /// </summary>
    /// <param name="useCustomLaunchOrder">是否使用自定义启动顺序</param>
    /// <param name="defaultLaunchIntervalSeconds">默认启动间隔时间(秒)</param>
    public void UpdateLaunchConfig(bool useCustomLaunchOrder, int defaultLaunchIntervalSeconds)
    {
        UseCustomLaunchOrder = useCustomLaunchOrder;
        DefaultLaunchIntervalSeconds = defaultLaunchIntervalSeconds >= 0 ? defaultLaunchIntervalSeconds : 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 获取启动顺序配置 (JSON 格式)
    /// 格式: [{ "projectId": 1, "order": 1, "intervalSeconds": 5 }, ...]
    /// </summary>
    public string? LaunchOrderJson { get; private set; }

    /// <summary>
    /// 设置项目启动顺序配置
    /// </summary>
    /// <param name="launchOrders">启动顺序配置列表</param>
    public void SetLaunchOrder(IEnumerable<WorkSpaceProjectLaunchOrder> launchOrders)
    {
        var ordersList = launchOrders.ToList();
        LaunchOrderJson = ordersList.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(ordersList)
            : null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 获取项目启动顺序配置
    /// </summary>
    /// <returns>启动顺序配置列表</returns>
    public IReadOnlyList<WorkSpaceProjectLaunchOrder> GetLaunchOrder()
    {
        if (string.IsNullOrEmpty(LaunchOrderJson))
            return Array.Empty<WorkSpaceProjectLaunchOrder>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<WorkSpaceProjectLaunchOrder>>(LaunchOrderJson)
                   ?? Array.Empty<WorkSpaceProjectLaunchOrder>();
        }
        catch
        {
            return Array.Empty<WorkSpaceProjectLaunchOrder>();
        }
    }
}

/// <summary>
/// 工作空间项目启动顺序配置
/// </summary>
public class WorkSpaceProjectLaunchOrder
{
    public long ProjectId { get; set; }
    public int Order { get; set; }
    public int? IntervalSeconds { get; set; }
}
