using ProjectHub.Application.DTOs;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 应用服务接口 - 定义用例
/// 作为 UI 层和领域层之间的桥梁
/// </summary>

/// <summary>
/// 项目管理应用服务接口
/// </summary>
public interface IProjectAppService
{
    /// <summary>
    /// 创建项目
    /// </summary>
    Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新项目基本信息
    /// </summary>
    Task<ProjectDto> UpdateAsync(UpdateProjectDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除项目
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取项目详情
    /// </summary>
    Task<ProjectDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有活跃项目 (未归档)
    /// </summary>
    Task<IReadOnlyList<ProjectDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动项目
    /// </summary>
    Task LaunchAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量启动项目
    /// </summary>
    Task LaunchMultipleAsync(IEnumerable<long> projectIds, int intervalSeconds = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置收藏状态
    /// </summary>
    Task SetFavoriteAsync(long id, bool isFavorite, CancellationToken cancellationToken = default);

    /// <summary>
    /// 扫描并更新项目的磁盘空间信息
    /// </summary>
    Task RefreshDiskSpaceInfoAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据工作文件夹 ID 获取项目列表
    /// </summary>
    Task<IReadOnlyList<ProjectDto>> GetByWorkFolderIdAsync(long workFolderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 工作文件夹管理应用服务接口
/// </summary>
public interface IWorkFolderAppService
{
    /// <summary>
    /// 创建工作文件夹
    /// </summary>
    Task<WorkFolderDto> CreateAsync(CreateWorkFolderDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作文件夹
    /// </summary>
    Task<WorkFolderDto> UpdateAsync(UpdateWorkFolderDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除工作文件夹
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有工作文件夹 (含项目数量)
    /// </summary>
    Task<IReadOnlyList<WorkFolderDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 将项目移动到工作文件夹
    /// </summary>
    Task MoveProjectToWorkFolderAsync(long projectId, long? workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加项目到文件夹
    /// </summary>
    Task AddProjectToFolderAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从文件夹移除项目
    /// </summary>
    Task RemoveProjectFromFolderAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加工作空间到文件夹
    /// </summary>
    Task AddWorkSpaceToFolderAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从文件夹移除工作空间
    /// </summary>
    Task RemoveWorkSpaceFromFolderAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定文件夹的子文件夹
    /// </summary>
    Task<IReadOnlyList<WorkFolderDto>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有根级文件夹（不包含子文件夹）
    /// </summary>
    Task<IReadOnlyList<WorkFolderDto>> GetRootFoldersAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 工作空间管理应用服务接口
/// </summary>
public interface IWorkSpaceAppService
{
    /// <summary>
    /// 创建工作空间
    /// </summary>
    Task<WorkSpaceDto> CreateAsync(CreateWorkSpaceDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作空间
    /// </summary>
    Task<WorkSpaceDto> UpdateAsync(UpdateWorkSpaceDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除工作空间
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间详情
    /// </summary>
    Task<WorkSpaceDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有工作空间 (含项目数量)
    /// </summary>
    Task<IReadOnlyList<WorkSpaceDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 记录工作空间被打开
    /// </summary>
    Task RecordOpenAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将项目移动到工作空间
    /// </summary>
    Task MoveProjectToWorkSpaceAsync(long projectId, long? workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间的项目启动设置
    /// </summary>
    Task<WorkSpaceProjectSettingsDto> GetProjectSettingsAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新工作空间的项目启动设置
    /// </summary>
    Task<WorkSpaceProjectSettingsDto> UpdateProjectSettingsAsync(UpdateWorkSpaceProjectSettingsDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置工作空间收藏状态
    /// </summary>
    Task SetFavoriteAsync(long id, bool isFavorite, CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动工作空间中的所有项目
    /// </summary>
    Task LaunchAllAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置工作空间的项目列表（全量替换）
    /// </summary>
    Task SetWorkSpaceProjectsAsync(long workSpaceId, IReadOnlyList<long> projectIds, IReadOnlyDictionary<long, int?>? intervalSecondsMap = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据工作文件夹 ID 获取工作空间列表
    /// </summary>
    Task<IReadOnlyList<WorkSpaceDto>> GetByWorkFolderIdAsync(long workFolderId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Shell 右键菜单管理服务接口
/// </summary>
public interface IShellContextMenuService
{
    /// <summary>
    /// 注册右键菜单
    /// </summary>
    void Register();

    /// <summary>
    /// 注销右键菜单
    /// </summary>
    void Unregister();

    /// <summary>
    /// 检查是否已注册
    /// </summary>
    bool IsRegistered();
}


