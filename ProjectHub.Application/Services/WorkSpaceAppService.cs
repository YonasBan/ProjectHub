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

    public WorkSpaceAppService(IWorkSpaceRepository workSpaceRepository)
    {
        _workSpaceRepository = workSpaceRepository;
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
            UpdatedAt = workSpace.UpdatedAt
        };
    }
}
