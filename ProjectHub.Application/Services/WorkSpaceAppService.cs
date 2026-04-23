using AutoMapper;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 工作空间应用服务实现
/// </summary>
public class WorkSpaceAppService : IWorkSpaceAppService
{
    private readonly IWorkSpaceRepository _workSpaceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAppService _projectAppService;
    private readonly IProjectWorkSpaceRepository _projectWorkSpaceRepository;
    private readonly ILogger<WorkSpaceAppService> _logger;
    private readonly IMapper _mapper;

    public WorkSpaceAppService(
        IWorkSpaceRepository workSpaceRepository,
        IProjectRepository projectRepository,
        IProjectAppService projectAppService,
        IProjectWorkSpaceRepository projectWorkSpaceRepository,
        ILogger<WorkSpaceAppService> logger,
        IMapper mapper)
    {
        _workSpaceRepository = workSpaceRepository;
        _projectRepository = projectRepository;
        _projectAppService = projectAppService;
        _projectWorkSpaceRepository = projectWorkSpaceRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<WorkSpaceDto> CreateAsync(CreateWorkSpaceDto input, CancellationToken cancellationToken = default)
    {
        // 检查名称是否已存在
        if (await _workSpaceRepository.ExistsByNameAsync(input.Name, cancellationToken))
        {
            throw new InvalidOperationException($"工作空间 '{input.Name}' 已存在");
        }

        var workSpace = WorkSpace.Create(input.Name, input.SortOrder, input.Description);
        
        if (input.IsFavorite)
        {
            workSpace.SetFavorite(true);
        }

        await _workSpaceRepository.AddAsync(workSpace, cancellationToken);

        return _mapper.Map<WorkSpaceDto>(workSpace);
    }

    public async Task<WorkSpaceDto> UpdateAsync(UpdateWorkSpaceDto input, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={input.Id}) 不存在");

        workSpace.UpdateInfo(input.Name, input.Description);

        if (input.IsFavorite.HasValue)
        {
            workSpace.SetFavorite(input.IsFavorite.Value);
        }

        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);

        return _mapper.Map<WorkSpaceDto>(workSpace);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={id}) 不存在");

        await _workSpaceRepository.DeleteAsync(workSpace, cancellationToken);
    }

    public async Task<WorkSpaceDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(id, cancellationToken);
        if (workSpace == null) return null;

        var folderIds = await _workSpaceRepository.GetWorkFolderIdsByWorkSpaceIdAsync(id, cancellationToken);
        return MapToDto(workSpace, folderIds);
    }

    public async Task<IReadOnlyList<WorkSpaceDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        var workSpaces = await _workSpaceRepository.GetAllWithProjectCountAsync(cancellationToken);
        return await MapWorkSpacesToDtosAsync(workSpaces, cancellationToken);
    }

    /// <summary>
    /// 将工作空间列表映射为 DTO 列表
    /// </summary>
    private async Task<IReadOnlyList<WorkSpaceDto>> MapWorkSpacesToDtosAsync(IEnumerable<WorkSpace> workSpaces, CancellationToken cancellationToken)
    {
        var result = new List<WorkSpaceDto>();
        foreach (var workSpace in workSpaces)
        {
            var folderIds = await _workSpaceRepository.GetWorkFolderIdsByWorkSpaceIdAsync(workSpace.Id, cancellationToken);
            var dto = MapToDto(workSpace, folderIds);
            result.Add(dto);
        }
        return result;
    }

    public async Task RecordOpenAsync(long id, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={id}) 不存在");

        workSpace.RecordOpen();
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);
    }

    public async Task MoveProjectToWorkSpaceAsync(long projectId, long? workSpaceId, CancellationToken cancellationToken = default)
    {
        // TODO: 实现项目与工作空间的关联逻辑
        // 这需要在 Infrastructure 层添加 IProjectWorkSpaceRepository 的实现
        await Task.CompletedTask;
    }

    public async Task<WorkSpaceProjectSettingsDto> GetProjectSettingsAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(workSpaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={workSpaceId}) 不存在");

        // 获取该工作空间包含的所有项目
        // TODO: 需要通过 WorkSpace-Project 关联获取项目列表
        // 获取工作空间中的所有项目关联
        var projectWorkSpaces = await _projectWorkSpaceRepository.GetByWorkSpaceIdAsync(workSpaceId, cancellationToken);
        
        // 获取项目详情
        var projectSettings = new List<WorkSpaceProjectSettingItemDto>();
        foreach (var pws in projectWorkSpaces)
        {
            var project = await _projectRepository.GetByIdAsync(pws.ProjectId, cancellationToken);
            if (project != null)
            {
                projectSettings.Add(new WorkSpaceProjectSettingItemDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    IsEnabled = pws.IsEnabled,
                    SortOrder = pws.SortOrder
                });
            }
        }

        return new WorkSpaceProjectSettingsDto
        {
            WorkSpaceId = workSpace.Id,
            WorkSpaceName = workSpace.Name,
            AllProjects = projectSettings.Select(p => new ProjectDto
            {
                Id = p.ProjectId,
                Name = p.ProjectName
            }).ToList(),
            ProjectSettings = projectSettings
        };
    }

    public async Task<WorkSpaceProjectSettingsDto> UpdateProjectSettingsAsync(UpdateWorkSpaceProjectSettingsDto input, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(input.WorkSpaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={input.WorkSpaceId}) 不存在");

        // 更新启动配置
        if (input.UseCustomLaunchOrder.HasValue || input.DefaultLaunchIntervalSeconds.HasValue)
        {
            workSpace.UpdateLaunchConfig(
                input.UseCustomLaunchOrder ?? workSpace.UseCustomLaunchOrder,
                input.DefaultLaunchIntervalSeconds ?? workSpace.DefaultLaunchIntervalSeconds);
            await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);
        }

        // 更新启动顺序配置
        if (input.LaunchOrders != null)
        {
            var launchOrders = _mapper.Map<List<WorkSpaceProjectLaunchOrder>>(input.LaunchOrders);
            workSpace.SetLaunchOrder(launchOrders);
            await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);
        }

        // 重新获取设置并返回
        return await GetProjectSettingsAsync(input.WorkSpaceId, cancellationToken);
    }

    public async Task SetFavoriteAsync(long id, bool isFavorite, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={id}) 不存在");

        workSpace.SetFavorite(isFavorite);
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);
    }

    public async Task LaunchAllAsync(long id, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={id}) 不存在");

        // 记录打开时间
        workSpace.RecordOpen();
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);

        // 获取工作空间中的所有项目 ID
        var projectIds = await _workSpaceRepository.GetProjectIdsByWorkSpaceIdAsync(id, cancellationToken);

        if (projectIds.Count == 0)
        {
            _logger.LogWarning("工作空间 '{WorkSpaceName}' (Id={WorkSpaceId}) 中没有项目", workSpace.Name, id);
            return;
        }

        // 获取启用的项目 ID 列表
        var enabledProjectIds = await _workSpaceRepository.GetEnabledProjectIdsByWorkSpaceIdAsync(id, cancellationToken);

        // 确定要启动的项目列表
        List<long> projectsToLaunch;
        if (enabledProjectIds.Count > 0)
        {
            // 只启动用户启用的项目
            projectsToLaunch = enabledProjectIds.ToList();
            _logger.LogInformation("工作空间 '{WorkSpaceName}' 将启动 {Count} 个已启用项目", workSpace.Name, projectsToLaunch.Count);
        }
        else
        {
            // 启动所有项目
            projectsToLaunch = projectIds.ToList();
            _logger.LogInformation("工作空间 '{WorkSpaceName}' 将启动所有 {Count} 个项目", workSpace.Name, projectsToLaunch.Count);
        }

        if (projectsToLaunch.Count == 0)
        {
            _logger.LogWarning("工作空间 '{WorkSpaceName}' 中没有启用的项目", workSpace.Name);
            return;
        }

        // 根据启动顺序配置排序
        if (workSpace.UseCustomLaunchOrder)
        {
            var launchOrders = workSpace.GetLaunchOrder().ToDictionary(o => o.ProjectId, o => o.Order);
            projectsToLaunch = projectsToLaunch
                .OrderBy(pid => launchOrders.GetValueOrDefault(pid, int.MaxValue))
                .ToList();
        }

        // 依次启动项目
        foreach (var projectId in projectsToLaunch)
        {
            try
            {
                await _projectAppService.LaunchAsync(projectId, cancellationToken);

                // 应用启动间隔（如果有配置）
                if (workSpace.DefaultLaunchIntervalSeconds > 0)
                {
                    await Task.Delay(workSpace.DefaultLaunchIntervalSeconds * 1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动项目 (Id={ProjectId}) 失败", projectId);
                // 继续启动其他项目
            }
        }

        _logger.LogInformation("工作空间 '{WorkSpaceName}' 的所有项目启动完成", workSpace.Name);
    }

    /// <summary>
    /// 设置工作空间的项目列表（全量替换）
    /// </summary>
    public async Task SetWorkSpaceProjectsAsync(long workSpaceId, IReadOnlyList<long> projectIds, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(workSpaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={workSpaceId}) 不存在");

        // 清除现有项目关联
        await _projectWorkSpaceRepository.RemoveAllProjectsFromWorkSpaceAsync(workSpaceId, cancellationToken);

        // 添加新的项目关联
        for (int i = 0; i < projectIds.Count; i++)
        {
            await _projectWorkSpaceRepository.AddProjectToWorkSpaceAsync(
                projectIds[i], workSpaceId, isEnabled: true, sortOrder: i, cancellationToken);
        }

        // 更新项目计数
        workSpace.UpdateProjectCount(projectIds.Count);
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);

        _logger.LogInformation("工作空间 '{WorkSpaceName}' 的项目列表已更新，共 {ProjectCount} 个项目", workSpace.Name, projectIds.Count);
    }

    public async Task<IReadOnlyList<WorkSpaceDto>> GetByWorkFolderIdAsync(long workFolderId, CancellationToken cancellationToken = default)
    {
        var workSpaces = await _workSpaceRepository.GetByWorkFolderIdAsync(workFolderId, cancellationToken);
        return await MapWorkSpacesToDtosAsync(workSpaces, cancellationToken);
    }

    private WorkSpaceDto MapToDto(WorkSpace workSpace, IReadOnlyList<long>? folderIds = null)
    {
        var dto = _mapper.Map<WorkSpaceDto>(workSpace);
        dto.WorkFolderIds = folderIds ?? [];
        dto.LaunchOrders = _mapper.Map<List<WorkSpaceProjectLaunchOrderDto>>(workSpace.GetLaunchOrder());
        return dto;
    }
}
