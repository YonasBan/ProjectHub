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
    /// 检查路径是否已存在
    /// </summary>
    Task<bool> ExistsByPathAsync(string path, CancellationToken cancellationToken = default);
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

    /// <summary>
    /// 检查项目与文件夹的关联是否已存在
    /// </summary>
    Task<bool> ExistsProjectFolderAssociationAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查工作空间与文件夹的关联是否已存在
    /// </summary>
    Task<bool> ExistsWorkSpaceFolderAssociationAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件夹中项目的最大排序值
    /// </summary>
    Task<int> GetMaxProjectSortOrderAsync(long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件夹中工作空间的最大排序值
    /// </summary>
    Task<int> GetMaxWorkSpaceSortOrderAsync(long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加项目-文件夹关联
    /// </summary>
    Task AddProjectAssociationAsync(ProjectWorkFolder association, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除项目-文件夹关联
    /// </summary>
    Task RemoveProjectAssociationAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加工作空间-文件夹关联
    /// </summary>
    Task AddWorkSpaceAssociationAsync(WorkSpaceWorkFolder association, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除工作空间-文件夹关联
    /// </summary>
    Task RemoveWorkSpaceAssociationAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定父文件夹的子文件夹
    /// </summary>
    Task<IReadOnlyList<WorkFolder>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有根级文件夹（ParentId为null）
    /// </summary>
    Task<IReadOnlyList<WorkFolder>> GetRootFoldersAsync(CancellationToken cancellationToken = default);
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
    /// 获取工作空间中的所有项目 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetProjectIdsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间中启用的项目 ID 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetEnabledProjectIdsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间关联的文件夹ID列表
    /// </summary>
    Task<IReadOnlyList<long>> GetWorkFolderIdsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据工作文件夹 ID 查询工作空间
    /// </summary>
    Task<IReadOnlyList<WorkSpace>> GetByWorkFolderIdAsync(long workFolderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 项目与工作空间关联仓储接口
/// </summary>
public interface IProjectWorkSpaceRepository
{
    /// <summary>
    /// 添加项目到工作空间
    /// </summary>
    Task AddProjectToWorkSpaceAsync(long projectId, long workSpaceId, bool isEnabled = true, int sortOrder = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从工作空间移除项目
    /// </summary>
    Task RemoveProjectFromWorkSpaceAsync(long projectId, long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置项目在工作空间中的启用状态
    /// </summary>
    Task SetProjectEnabledAsync(long projectId, long workSpaceId, bool isEnabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间中的所有项目关联
    /// </summary>
    Task<IReadOnlyList<ProjectWorkSpace>> GetByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除工作空间的所有项目关联
    /// </summary>
    Task RemoveAllProjectsFromWorkSpaceAsync(long workSpaceId, CancellationToken cancellationToken = default);
}


