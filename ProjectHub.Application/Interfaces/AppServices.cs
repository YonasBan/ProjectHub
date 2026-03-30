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
    /// 获取最近使用的项目
    /// </summary>
    Task<IReadOnlyList<ProjectDto>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索项目
    /// </summary>
    Task<IReadOnlyList<ProjectDto>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

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
    /// 获取所有工作空间 (含项目数量)
    /// </summary>
    Task<IReadOnlyList<WorkSpaceDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近打开的工作空间
    /// </summary>
    Task<IReadOnlyList<WorkSpaceDto>> GetRecentlyOpenedAsync(int count, CancellationToken cancellationToken = default);

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
}

/// <summary>
/// 垃圾清理应用服务接口
/// </summary>
public interface ICleanupAppService
{
    /// <summary>
    /// 扫描单个项目的垃圾文件
    /// </summary>
    Task<CleanupScanResultDto> ScanProjectAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 扫描多个项目的垃圾文件
    /// </summary>
    Task<CleanupScanResultDto> ScanProjectsAsync(IEnumerable<long> projectIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行清理
    /// </summary>
    Task<long> ExecuteCleanupAsync(IEnumerable<string> pathsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取清理预设方案
    /// </summary>
    IEnumerable<CleanupProfileDto> GetCleanupProfiles();
}

/// <summary>
/// 标签管理应用服务接口
/// </summary>
public interface ITagAppService
{
    /// <summary>
    /// 创建标签
    /// </summary>
    Task<TagDto> CreateAsync(CreateTagDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新标签
    /// </summary>
    Task<TagDto> UpdateAsync(UpdateTagDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除标签
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 为项目添加标签
    /// </summary>
    Task AddTagToProjectAsync(long projectId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从项目移除标签
    /// </summary>
    Task RemoveTagFromProjectAsync(long projectId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    Task<IReadOnlyList<TagDto>> GetTagsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 为工作空间添加标签
    /// </summary>
    Task AddTagToWorkSpaceAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从工作空间移除标签
    /// </summary>
    Task RemoveTagFromWorkSpaceAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取工作空间的标签列表
    /// </summary>
    Task<IReadOnlyList<TagDto>> GetTagsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签 ID 获取项目
    /// </summary>
    Task<IReadOnlyList<ProjectDto>> GetProjectsByTagIdAsync(long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标签 ID 获取工作空间
    /// </summary>
    Task<IReadOnlyList<WorkSpaceDto>> GetWorkSpacesByTagIdAsync(long tagId, CancellationToken cancellationToken = default);
}
