using ProjectHub.Domain.Entities;
using ProjectHub.Domain.ValueObjects;

namespace ProjectHub.Domain.Interfaces;

/// <summary>
/// 通用仓储接口 (Repository Pattern)
/// 定义 CRUD 基本操作
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// 根据 ID 获取实体
    /// </summary>
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加实体
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新实体
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除实体
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目仓储接口
/// 扩展特定查询方法
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// 根据分组 ID 查询项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetByGroupIdAsync(long? groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签 ID 查询项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetByTagIdAsync(long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索项目 (按名称/路径/描述)
    /// </summary>
    Task<IReadOnlyList<Project>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近使用的项目
    /// </summary>
    /// <param name="count">返回数量</param>
    Task<IReadOnlyList<Project>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取未归档的项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取已归档的项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetArchivedProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏的项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetFavoriteProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查路径是否已存在
    /// </summary>
    Task<bool> ExistsByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据类型筛选项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetByTypeAsync(ProjectType type, CancellationToken cancellationToken = default);
}

/// <summary>
/// 分组仓储接口
/// </summary>
public interface IGroupRepository : IRepository<Group>
{
    /// <summary>
    /// 获取所有分组 (包含项目数量统计)
    /// </summary>
    Task<IReadOnlyList<Group>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查分组名称是否已存在
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// 标签仓储接口
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>
    /// 获取所有标签 (包含项目数量统计)
    /// </summary>
    Task<IReadOnlyList<Tag>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查标签名称是否已存在
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目包仓储接口
/// </summary>
public interface IProjectBundleRepository
{
    /// <summary>
    /// 根据 ID 获取项目包
    /// </summary>
    Task<ProjectBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有项目包
    /// </summary>
    Task<IReadOnlyList<ProjectBundle>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加项目包
    /// </summary>
    Task<ProjectBundle> AddAsync(ProjectBundle bundle, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新项目包
    /// </summary>
    Task UpdateAsync(ProjectBundle bundle, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除项目包
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
