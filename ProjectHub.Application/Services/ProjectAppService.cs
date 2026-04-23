using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Services.LaunchProviders;
using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 项目应用服务实现
/// </summary>
public class ProjectAppService : IProjectAppService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProcessLauncherService _processLauncher;
    private readonly IMapper _mapper;
    private readonly LaunchProviderFactory _launchProviderFactory;

    public ProjectAppService(
        IProjectRepository projectRepository,
        IProcessLauncherService processLauncher,
        IMapper mapper,
        LaunchProviderFactory launchProviderFactory)
    {
        _projectRepository = projectRepository;
        _processLauncher = processLauncher;
        _mapper = mapper;
        _launchProviderFactory = launchProviderFactory;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default)
    {
        // 根据启动类型确定项目路径
        var projectPath = input.LaunchType switch
        {
            Domain.Entities.LaunchType.OpenFile => input.Path,
            Domain.Entities.LaunchType.OpenExe => input.DefaultProgram ?? string.Empty,
            Domain.Entities.LaunchType.OpenWebUrl => input.WebUrl ?? string.Empty,
            Domain.Entities.LaunchType.OpenCmd => input.CmdCommand ?? string.Empty,
            _ => input.Path
        };

        var project = Domain.Entities.Project.Create(
            input.Name, 
            projectPath,
            input.DefaultProgram,
            input.CustomIconPath,
            input.Description);
        
        // 使用 AutoMapper 映射其余字段
        _mapper.Map(input, project);

        await _projectRepository.AddAsync(project, cancellationToken);
        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> UpdateAsync(UpdateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={input.Id}) not found");

        // 使用 AutoMapper 映射更新字段
        _mapper.Map(input, project);

        await _projectRepository.UpdateAsync(project, cancellationToken);
        return _mapper.Map<ProjectDto>(project);
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
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<ProjectDto>>(projects);
    }

    public async Task LaunchAsync(long id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={id}) not found");

        // 启动项目
        await LaunchProjectAsync(project);

        // 记录启动
        project.RecordLaunch();
        await _projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <summary>
    /// 启动项目的实际逻辑
    /// 使用工厂模式获取对应的启动提供者
    /// </summary>
    private async Task LaunchProjectAsync(Domain.Entities.Project project)
    {
        // 使用工厂获取对应的启动提供者
        var provider = _launchProviderFactory.GetProvider(project.LaunchType);
        
        // 执行启动
        await provider.LaunchAsync(project);
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

    public async Task<IReadOnlyList<ProjectDto>> GetByWorkFolderIdAsync(long workFolderId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetByWorkFolderIdAsync(workFolderId, cancellationToken);
        return _mapper.Map<List<ProjectDto>>(projects);
    }
}
