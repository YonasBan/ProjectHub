using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 标签仓储实现
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly AppDbContext _dbContext;

    public TagRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tag?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags.ToListAsync(cancellationToken);
    }

    public async Task<Tag> AddAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tags.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有标签 (按排序顺序)
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 检查标签名称是否已存在
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AnyAsync(t => t.Name == name, cancellationToken);
    }

    /// <summary>
    /// 根据标签 ID 获取关联的项目 ID 列表
    /// </summary>
    public async Task<IReadOnlyList<long>> GetProjectIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectTags
            .Where(pt => pt.TagId == tagId)
            .Select(pt => pt.ProjectId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据标签 ID 获取关联的工作空间 ID 列表
    /// </summary>
    public async Task<IReadOnlyList<long>> GetWorkSpaceIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkSpaceTags
            .Where(wst => wst.TagId == tagId)
            .Select(wst => wst.WorkSpaceId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetTagsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var tagIds = await _dbContext.ProjectTags
            .Where(pt => pt.ProjectId == projectId)
            .Select(pt => pt.TagId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Tags
            .Where(t => tagIds.Contains(t.Id))
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取工作空间的标签列表
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetTagsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        var tagIds = await _dbContext.WorkSpaceTags
            .Where(wst => wst.WorkSpaceId == workSpaceId)
            .Select(wst => wst.TagId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Tags
            .Where(t => tagIds.Contains(t.Id))
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 添加项目-标签关联
    /// </summary>
    public async Task<ProjectTag> AddProjectTagAsync(ProjectTag projectTag, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProjectTags.AddAsync(projectTag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return projectTag;
    }

    /// <summary>
    /// 删除项目-标签关联
    /// </summary>
    public async Task DeleteProjectTagAsync(long projectId, long tagId, CancellationToken cancellationToken = default)
    {
        var projectTag = await _dbContext.ProjectTags
            .FirstOrDefaultAsync(pt => pt.ProjectId == projectId && pt.TagId == tagId, cancellationToken);

        if (projectTag != null)
        {
            _dbContext.ProjectTags.Remove(projectTag);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 添加工作空间-标签关联
    /// </summary>
    public async Task<WorkSpaceTag> AddWorkSpaceTagAsync(WorkSpaceTag workSpaceTag, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkSpaceTags.AddAsync(workSpaceTag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return workSpaceTag;
    }

    /// <summary>
    /// 删除工作空间-标签关联
    /// </summary>
    public async Task DeleteWorkSpaceTagAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default)
    {
        var workSpaceTag = await _dbContext.WorkSpaceTags
            .FirstOrDefaultAsync(wst => wst.WorkSpaceId == workSpaceId && wst.TagId == tagId, cancellationToken);

        if (workSpaceTag != null)
        {
            _dbContext.WorkSpaceTags.Remove(workSpaceTag);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
