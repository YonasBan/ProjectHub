using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 标签应用服务实现
/// </summary>
public class TagAppService : ITagAppService
{
    private readonly ITagRepository _tagRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkSpaceRepository _workSpaceRepository;
    private readonly ILogger<TagAppService> _logger;

    public TagAppService(
        ITagRepository tagRepository,
        IProjectRepository projectRepository,
        IWorkSpaceRepository workSpaceRepository,
        ILogger<TagAppService> logger)
    {
        _tagRepository = tagRepository;
        _projectRepository = projectRepository;
        _workSpaceRepository = workSpaceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 创建标签
    /// </summary>
    public async Task<TagDto> CreateAsync(CreateTagDto input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建标签: {TagName}", input.Name);

        // 检查名称是否已存在
        if (await _tagRepository.ExistsByNameAsync(input.Name, cancellationToken))
        {
            throw new InvalidOperationException($"标签名称 '{input.Name}' 已存在");
        }

        var tag = Tag.Create(input.Name, input.Color, input.Description, input.SortOrder);
        await _tagRepository.AddAsync(tag, cancellationToken);

        _logger.LogInformation("标签创建成功: {TagId}", tag.Id);
        return MapToDto(tag);
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    public async Task<TagDto> UpdateAsync(UpdateTagDto input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新标签: {TagId}", input.Id);

        var tag = await _tagRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"标签 (Id={input.Id}) 不存在");

        tag.UpdateInfo(input.Name, input.Color, input.Description);
        if (input.SortOrder.HasValue)
        {
            tag.UpdateSortOrder(input.SortOrder.Value);
        }

        await _tagRepository.UpdateAsync(tag, cancellationToken);

        _logger.LogInformation("标签更新成功: {TagId}", tag.Id);
        return MapToDto(tag);
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除标签: {TagId}", id);

        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"标签 (Id={id}) 不存在");

        await _tagRepository.DeleteAsync(tag, cancellationToken);

        _logger.LogInformation("标签删除成功: {TagId}", id);
    }

    /// <summary>
    /// 获取所有标签
    /// </summary>
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllOrderedAsync(cancellationToken);
        var result = new List<TagDto>();

        foreach (var tag in tags)
        {
            var projectIds = await _tagRepository.GetProjectIdsByTagIdAsync(tag.Id, cancellationToken);
            var workSpaceIds = await _tagRepository.GetWorkSpaceIdsByTagIdAsync(tag.Id, cancellationToken);

            result.Add(new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color,
                Description = tag.Description,
                SortOrder = tag.SortOrder,
                CreatedAt = tag.CreatedAt,
                UpdatedAt = tag.UpdatedAt,
                ProjectCount = projectIds.Count,
                WorkSpaceCount = workSpaceIds.Count
            });
        }

        return result;
    }

    /// <summary>
    /// 为项目添加标签
    /// </summary>
    public async Task AddTagToProjectAsync(long projectId, long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("为项目添加标签: ProjectId={ProjectId}, TagId={TagId}", projectId, tagId);

        // 验证项目是否存在
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"项目 (Id={projectId}) 不存在");
        }

        // 验证标签是否存在
        var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"标签 (Id={tagId}) 不存在");
        }

        // 检查是否已经关联
        var existingTags = await _tagRepository.GetTagsByProjectIdAsync(projectId, cancellationToken);
        if (existingTags.Any(t => t.Id == tagId))
        {
            _logger.LogWarning("项目已关联该标签: ProjectId={ProjectId}, TagId={TagId}", projectId, tagId);
            return;
        }

        var projectTag = ProjectTag.Create(projectId, tagId);
        await _tagRepository.AddProjectTagAsync(projectTag, cancellationToken);

        _logger.LogInformation("项目标签添加成功: ProjectId={ProjectId}, TagId={TagId}", projectId, tagId);
    }

    /// <summary>
    /// 从项目移除标签
    /// </summary>
    public async Task RemoveTagFromProjectAsync(long projectId, long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从项目移除标签: ProjectId={ProjectId}, TagId={TagId}", projectId, tagId);

        await _tagRepository.DeleteProjectTagAsync(projectId, tagId, cancellationToken);

        _logger.LogInformation("项目标签移除成功: ProjectId={ProjectId}, TagId={TagId}", projectId, tagId);
    }

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    public async Task<IReadOnlyList<TagDto>> GetTagsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetTagsByProjectIdAsync(projectId, cancellationToken);
        return tags.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 为工作空间添加标签
    /// </summary>
    public async Task AddTagToWorkSpaceAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("为工作空间添加标签: WorkSpaceId={WorkSpaceId}, TagId={TagId}", workSpaceId, tagId);

        // 验证工作空间是否存在
        var workSpace = await _workSpaceRepository.GetByIdAsync(workSpaceId, cancellationToken);
        if (workSpace == null)
        {
            throw new KeyNotFoundException($"工作空间 (Id={workSpaceId}) 不存在");
        }

        // 验证标签是否存在
        var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
        if (tag == null)
        {
            throw new KeyNotFoundException($"标签 (Id={tagId}) 不存在");
        }

        // 检查是否已经关联
        var existingTags = await _tagRepository.GetTagsByWorkSpaceIdAsync(workSpaceId, cancellationToken);
        if (existingTags.Any(t => t.Id == tagId))
        {
            _logger.LogWarning("工作空间已关联该标签: WorkSpaceId={WorkSpaceId}, TagId={TagId}", workSpaceId, tagId);
            return;
        }

        var workSpaceTag = WorkSpaceTag.Create(workSpaceId, tagId);
        await _tagRepository.AddWorkSpaceTagAsync(workSpaceTag, cancellationToken);

        _logger.LogInformation("工作空间标签添加成功: WorkSpaceId={WorkSpaceId}, TagId={TagId}", workSpaceId, tagId);
    }

    /// <summary>
    /// 从工作空间移除标签
    /// </summary>
    public async Task RemoveTagFromWorkSpaceAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从工作空间移除标签: WorkSpaceId={WorkSpaceId}, TagId={TagId}", workSpaceId, tagId);

        await _tagRepository.DeleteWorkSpaceTagAsync(workSpaceId, tagId, cancellationToken);

        _logger.LogInformation("工作空间标签移除成功: WorkSpaceId={WorkSpaceId}, TagId={TagId}", workSpaceId, tagId);
    }

    /// <summary>
    /// 获取工作空间的标签列表
    /// </summary>
    public async Task<IReadOnlyList<TagDto>> GetTagsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetTagsByWorkSpaceIdAsync(workSpaceId, cancellationToken);
        return tags.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 根据标签 ID 获取项目
    /// </summary>
    public async Task<IReadOnlyList<ProjectDto>> GetProjectsByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("根据标签获取项目: TagId={TagId}", tagId);

        var projectIds = await _tagRepository.GetProjectIdsByTagIdAsync(tagId, cancellationToken);
        var projects = new List<ProjectDto>();

        foreach (var projectId in projectIds)
        {
            var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
            if (project != null)
            {
                projects.Add(MapToProjectDto(project));
            }
        }

        return projects;
    }

    /// <summary>
    /// 根据标签 ID 获取工作空间
    /// </summary>
    public async Task<IReadOnlyList<WorkSpaceDto>> GetWorkSpacesByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("根据标签获取工作空间: TagId={TagId}", tagId);

        var workSpaceIds = await _tagRepository.GetWorkSpaceIdsByTagIdAsync(tagId, cancellationToken);
        var workSpaces = new List<WorkSpaceDto>();

        foreach (var workSpaceId in workSpaceIds)
        {
            var workSpace = await _workSpaceRepository.GetByIdAsync(workSpaceId, cancellationToken);
            if (workSpace != null)
            {
                workSpaces.Add(MapToWorkSpaceDto(workSpace));
            }
        }

        return workSpaces;
    }

    /// <summary>
    /// 映射为 TagDto
    /// </summary>
    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            Description = tag.Description,
            SortOrder = tag.SortOrder,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt,
            ProjectCount = 0,
            WorkSpaceCount = 0
        };
    }

    /// <summary>
    /// 映射为 ProjectDto
    /// </summary>
    private static ProjectDto MapToProjectDto(Project project)
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

    /// <summary>
    /// 映射为 WorkSpaceDto
    /// </summary>
    private static WorkSpaceDto MapToWorkSpaceDto(WorkSpace workSpace)
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
            EnabledProjectIds = workSpace.GetEnabledProjectIds().ToList(),
            CreatedAt = workSpace.CreatedAt,
            UpdatedAt = workSpace.UpdatedAt
        };
    }
}
