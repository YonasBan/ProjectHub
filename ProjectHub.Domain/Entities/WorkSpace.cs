
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
    /// 已启用启动的项目 ID 列表 (JSON 格式存储)
    /// 用于记录用户勾选的会自动启动的项目
    /// </summary>
    public string? EnabledProjectIdsJson { get; private set; }

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
            IsFavorite = false
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
    /// 设置已启用启动的项目 ID 列表
    /// </summary>
    /// <param name="projectIds">项目 ID 列表</param>
    public void SetEnabledProjectIds(IEnumerable<long> projectIds)
    {
        var idsList = projectIds.ToList();
        EnabledProjectIdsJson = idsList.Count > 0 
            ? System.Text.Json.JsonSerializer.Serialize(idsList.Distinct().OrderBy(x => x))
            : null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 获取已启用启动的项目 ID 列表
    /// </summary>
    /// <returns>项目 ID 列表</returns>
    public IEnumerable<long> GetEnabledProjectIds()
    {
        if (string.IsNullOrEmpty(EnabledProjectIdsJson))
            return Enumerable.Empty<long>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<long>>(EnabledProjectIdsJson) 
                   ?? Enumerable.Empty<long>();
        }
        catch
        {
            return Enumerable.Empty<long>();
        }
    }

    /// <summary>
    /// 添加单个项目到启用列表
    /// </summary>
    /// <param name="projectId">项目 ID</param>
    public void AddEnabledProject(long projectId)
    {
        var enabledIds = GetEnabledProjectIds().ToList();
        if (!enabledIds.Contains(projectId))
        {
            enabledIds.Add(projectId);
            SetEnabledProjectIds(enabledIds);
        }
    }

    /// <summary>
    /// 从启用列表中移除单个项目
    /// </summary>
    /// <param name="projectId">项目 ID</param>
    public void RemoveEnabledProject(long projectId)
    {
        var enabledIds = GetEnabledProjectIds().ToList();
        if (enabledIds.Contains(projectId))
        {
            enabledIds.Remove(projectId);
            SetEnabledProjectIds(enabledIds);
        }
    }
}
