using AutoMapper;
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
    private readonly IProcessLauncherService _processLauncher;
    private readonly IMapper _mapper;

    public ProjectAppService(
        IProjectRepository projectRepository,
        IProcessLauncherService processLauncher,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _processLauncher = processLauncher;
        _mapper = mapper;
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = Domain.Entities.Project.Create(input.Name, input.Type, input.Path,input.DefaultProgram,input.CustomIconPath,input.Description);
        
        // 设置可选字段
        if (!string.IsNullOrWhiteSpace(input.LaunchArguments))
        {
            project.SetLaunchArguments(input.LaunchArguments);
        }
        
        await _projectRepository.AddAsync(project, cancellationToken);
        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> UpdateAsync(UpdateProjectDto input, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project (Id={input.Id}) not found");

        // 更新基本信息
        project.UpdateBasicInfo(input.Name, input.Description, null, input.DefaultProgram, input.LaunchArguments, input.CustomIconPath);

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

    public async Task<IReadOnlyList<ProjectDto>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetRecentlyUsedAsync(count, cancellationToken);
        return _mapper.Map<List<ProjectDto>>(projects);
    }

    public async Task<IReadOnlyList<ProjectDto>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.SearchAsync(keyword, cancellationToken);
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
    /// </summary>
    private async Task LaunchProjectAsync(Domain.Entities.Project project)
    {
        var projectPath = project.Path;
        
        // 检查路径是否存在
        if (!_processLauncher.FileExists(projectPath))
        {
            throw new FileNotFoundException($"Project path not found: {projectPath}");
        }

        // 如果有 DefaultProgram，使用它启动
        if (!string.IsNullOrEmpty(project.DefaultProgram))
        {
            await _processLauncher.LaunchWithProgramAsync(project.DefaultProgram, projectPath);
            return;
        }

        // 使用系统默认方式启动
        await _processLauncher.LaunchWithDefaultProgramAsync(projectPath);
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
