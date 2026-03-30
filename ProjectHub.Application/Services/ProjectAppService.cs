using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 项目应用服务实现
/// </summary>
public class ProjectAppService : IProjectAppService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectAppService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = Domain.Entities.Project.Create(input.Name, input.Type, input.Path);
        await _projectRepository.AddAsync(project, cancellationToken);
        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(UpdateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={input.Id}) not found");

        // TODO: 实现更新逻辑
        return MapToDto(project);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={id}) not found");

        await _projectRepository.DeleteAsync(project, cancellationToken);
    }

    public async Task<ProjectDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project == null ? null : MapToDto(project);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ProjectDto>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetRecentlyUsedAsync(count, cancellationToken);
        return projects.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ProjectDto>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.SearchAsync(keyword, cancellationToken);
        return projects.Select(MapToDto).ToList();
    }

    public async Task LaunchAsync(long id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={id}) not found");

        project.RecordLaunch();
        await _projectRepository.UpdateAsync(project, cancellationToken);
        
        // TODO: 实际启动项目的逻辑
    }

    public async Task LaunchMultipleAsync(IEnumerable<long> projectIds, int intervalSeconds = 0, CancellationToken cancellationToken = default)
    {
        foreach (var id in projectIds)
        {
            await LaunchAsync(id, cancellationToken);
            if (intervalSeconds > 0)
            {
                await Task.Delay(intervalSeconds * 1000, cancellationToken);
            }
        }
    }

    public async Task SetFavoriteAsync(long id, bool isFavorite, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={id}) not found");

        project.SetFavorite(isFavorite);
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }

    public async Task RefreshDiskSpaceInfoAsync(long id, CancellationToken cancellationToken = default)
    {
        // TODO: 实现磁盘空间扫描
        await Task.CompletedTask;
    }

    private static ProjectDto MapToDto(Domain.Entities.Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Type = project.Type,
            Path = project.Path,
            CustomIconPath = project.CustomIconPath,
            Description = project.Description,
            LaunchCount = project.LaunchCount,
            TotalUsageDurationMs = project.TotalUsageDurationMs,
            LastOpenedAt = project.LastOpenedAt,
            IsFavorite = project.IsFavorite,
            DiskSpaceBytes = project.DiskSpaceBytes,
            CleanableSpaceBytes = project.CleanableSpaceBytes,
            Deadline = project.Deadline,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
