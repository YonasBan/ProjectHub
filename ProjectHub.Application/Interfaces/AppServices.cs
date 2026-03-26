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
    /// 归档项目
    /// </summary>
    Task ArchiveAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 恢复已归档项目
    /// </summary>
    Task RestoreFromArchiveAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置收藏状态
    /// </summary>
    Task SetFavoriteAsync(long id, bool isFavorite, CancellationToken cancellationToken = default);

    /// <summary>
    /// 为项目添加标签
    /// </summary>
    Task AddTagAsync(long projectId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除项目标签
    /// </summary>
    Task RemoveTagAsync(long projectId, long tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    Task<IReadOnlyList<TagDto>> GetTagsAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 扫描并更新项目的磁盘空间信息
    /// </summary>
    Task RefreshDiskSpaceInfoAsync(long id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 分组管理应用服务接口
/// </summary>
public interface IGroupAppService
{
    /// <summary>
    /// 创建分组
    /// </summary>
    Task<GroupDto> CreateAsync(CreateGroupDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新分组
    /// </summary>
    Task<GroupDto> UpdateAsync(UpdateGroupDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除分组
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有分组 (含项目数量)
    /// </summary>
    Task<IReadOnlyList<GroupDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 将项目移动到分组
    /// </summary>
    Task MoveProjectToGroupAsync(long projectId, long? groupId, CancellationToken cancellationToken = default);
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
    /// 获取所有标签 (含项目数量)
    /// </summary>
    Task<IReadOnlyList<TagDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 合并标签
    /// </summary>
    Task MergeTagsAsync(long sourceTagId, long targetTagId, CancellationToken cancellationToken = default);
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
/// 项目包管理应用服务接口
/// </summary>
public interface IProjectBundleAppService
{
    /// <summary>
    /// 创建项目包
    /// </summary>
    Task<ProjectBundleDto> CreateAsync(CreateProjectBundleDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新项目包
    /// </summary>
    Task<ProjectBundleDto> UpdateAsync(UpdateProjectBundleDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除项目包
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有项目包
    /// </summary>
    Task<IReadOnlyList<ProjectBundleDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 启动项目包
    /// </summary>
    Task LaunchBundleAsync(Guid id, CancellationToken cancellationToken = default);
}
