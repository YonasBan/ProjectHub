using ProjectHub.Domain.Entities;

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
    /// 根据工作文件夹 ID 查询项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetByWorkFolderIdAsync(long? workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据工作空间 ID 查询项目
    /// </summary>
    Task<IReadOnlyList<Project>> GetByWorkSpaceIdAsync(long? workSpaceId, CancellationToken cancellationToken = default);

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
/// 工作文件夹仓储接口
/// </summary>
public interface IWorkFolderRepository : IRepository<WorkFolder>
{
    /// <summary>
    /// 获取所有工作文件夹 (包含项目数量统计)
    /// </summary>
    Task<IReadOnlyList<WorkFolder>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查工作文件夹名称是否已存在
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查同级目录下是否存在同名文件夹
    /// </summary>
    /// <param name="name">文件夹名称</param>
    /// <param name="parentId">父文件夹ID（null表示根级）</param>
    Task<bool> ExistsByNameAndParentIdAsync(string name, long? parentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 工作空间仓储接口
/// </summary>
public interface IWorkSpaceRepository : IRepository<WorkSpace>
{
    /// <summary>
    /// 获取所有工作空间 (包含项目数量统计)
    /// </summary>
    Task<IReadOnlyList<WorkSpace>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查工作空间名称是否已存在
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近打开的工作空间
    /// </summary>
    /// <param name="count">返回数量</param>
    Task<IReadOnlyList<WorkSpace>> GetRecentlyOpenedAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间中的所有项目 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetProjectIdsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间中启用的项目 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetEnabledProjectIdsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 标签仓储接口
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>
    /// 获取所有标签 (按排序顺序)
    /// </summary>
    Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查标签名称是否已存在
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签 ID 获取关联的项目 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetProjectIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签 ID 获取关联的工作空间 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetWorkSpaceIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    Task<IReadOnlyList<Tag>> GetTagsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间的标签列表
    /// </summary>
    Task<IReadOnlyList<Tag>> GetTagsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加项目-标签关联
    /// </summary>
    Task<ProjectTag> AddProjectTagAsync(ProjectTag projectTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除项目-标签关联
    /// </summary>
    Task DeleteProjectTagAsync(long projectId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加工作空间-标签关联
    /// </summary>
    Task<WorkSpaceTag> AddWorkSpaceTagAsync(WorkSpaceTag workSpaceTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除工作空间-标签关联
    /// </summary>
    Task DeleteWorkSpaceTagAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default);
}
