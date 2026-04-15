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

    public WorkSpaceAppService(IWorkSpaceRepository workSpaceRepository, IProjectRepository projectRepository)
    {
        _workSpaceRepository = workSpaceRepository;
        _projectRepository = projectRepository;
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

        return MapToDto(workSpace);
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

        return MapToDto(workSpace);
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
        return workSpace == null ? null : MapToDto(workSpace);
    }

    public async Task<IReadOnlyList<WorkSpaceDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        var workSpaces = await _workSpaceRepository.GetAllWithProjectCountAsync(cancellationToken);
        return workSpaces.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<WorkSpaceDto>> GetRecentlyOpenedAsync(int count, CancellationToken cancellationToken = default)
    {
        var workSpaces = await _workSpaceRepository.GetRecentlyOpenedAsync(count, cancellationToken);
        return workSpaces.Select(MapToDto).ToList();
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
        // 暂时返回空列表，后续需要实现关联查询
        var allProjects = new List<ProjectDto>();
        
        return new WorkSpaceProjectSettingsDto
        {
            WorkSpaceId = workSpace.Id,
            WorkSpaceName = workSpace.Name,
            AllProjects = allProjects,
            EnabledProjectIds = workSpace.GetEnabledProjectIds().ToList()
        };
    }

    public async Task<WorkSpaceProjectSettingsDto> UpdateProjectSettingsAsync(UpdateWorkSpaceProjectSettingsDto input, CancellationToken cancellationToken = default)
    {
        var workSpace = await _workSpaceRepository.GetByIdAsync(input.WorkSpaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"工作空间 (Id={input.WorkSpaceId}) 不存在");

        // 更新已启用启动的项目 ID 列表
        workSpace.SetEnabledProjectIds(input.EnabledProjectIds);
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);

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

        // TODO: 获取工作空间中的所有项目并依次启动
        // 暂时记录打开时间
        workSpace.RecordOpen();
        await _workSpaceRepository.UpdateAsync(workSpace, cancellationToken);

        // 实际启动逻辑需要获取关联的项目列表并调用 IProjectAppService.LaunchAsync
        await Task.CompletedTask;
    }

    private static WorkSpaceDto MapToDto(WorkSpace workSpace)
    {
        return new WorkSpaceDto
        {
            Id = workSpace.Id,
            Name = workSpace.Name,
            Description = workSpace.Description,
            IconPath = workSpace.IconPath,
            SortOrder = workSpace.SortOrder,
            ProjectCount = workSpace.ProjectCount,
            IsFavorite = workSpace.IsFavorite,
            FavoritedAt = workSpace.FavoritedAt,
            LastOpenedAt = workSpace.LastOpenedAt,
            CreatedAt = workSpace.CreatedAt,
            UpdatedAt = workSpace.UpdatedAt,
            EnabledProjectIds = workSpace.GetEnabledProjectIds().ToList(),
            UseCustomLaunchOrder = workSpace.UseCustomLaunchOrder,
            DefaultLaunchIntervalSeconds = workSpace.DefaultLaunchIntervalSeconds,
            LaunchOrders = workSpace.GetLaunchOrder()
                .Select(o => new WorkSpaceProjectLaunchOrderDto
                {
                    ProjectId = o.ProjectId,
                    Order = o.Order,
                    IntervalSeconds = o.IntervalSeconds
                })
                .ToList()
        };
    }
}
