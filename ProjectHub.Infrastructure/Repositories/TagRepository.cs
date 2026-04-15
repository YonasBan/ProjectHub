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
    private readonly IDbContextFactory<AppDbContext> _factory;

    public TagRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Tag?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Tags
            .Where(t => !t.IsDeleted)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Tags
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag> AddAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.Tags.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        ctx.Tags.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tag entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        entity.SoftDelete();
        ctx.Tags.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有标签 (按排序顺序)
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Tags
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 检查标签名称是否已存在
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Tags
            .Where(t => !t.IsDeleted)
            .AnyAsync(t => t.Name == name, cancellationToken);
    }

    /// <summary>
    /// 根据标签 ID 获取关联的项目 ID 列表
    /// </summary>
    public async Task<IReadOnlyList<long>> GetProjectIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.ProjectTags
            .Where(pt => pt.TagId == tagId)
            .Select(pt => pt.ProjectId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 根据标签 ID 获取关联的工作空间 ID 列表
    /// </summary>
    public async Task<IReadOnlyList<long>> GetWorkSpaceIdsByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkSpaceTags
            .Where(wst => wst.TagId == tagId)
            .Select(wst => wst.WorkSpaceId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取项目的标签列表
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetTagsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var tagIds = await ctx.ProjectTags
            .Where(pt => pt.ProjectId == projectId)
            .Select(pt => pt.TagId)
            .ToListAsync(cancellationToken);

        return await ctx.Tags
            .Where(t => !t.IsDeleted && tagIds.Contains(t.Id))
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取工作空间的标签列表
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetTagsByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var tagIds = await ctx.WorkSpaceTags
            .Where(wst => wst.WorkSpaceId == workSpaceId)
            .Select(wst => wst.TagId)
            .ToListAsync(cancellationToken);

        return await ctx.Tags
            .Where(t => !t.IsDeleted && tagIds.Contains(t.Id))
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 添加项目-标签关联
    /// </summary>
    public async Task<ProjectTag> AddProjectTagAsync(ProjectTag projectTag, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.ProjectTags.AddAsync(projectTag, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return projectTag;
    }

    /// <summary>
    /// 删除项目-标签关联
    /// </summary>
    public async Task DeleteProjectTagAsync(long projectId, long tagId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var projectTag = await ctx.ProjectTags
            .FirstOrDefaultAsync(pt => pt.ProjectId == projectId && pt.TagId == tagId, cancellationToken);

        if (projectTag != null)
        {
            ctx.ProjectTags.Remove(projectTag);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 添加工作空间-标签关联
    /// </summary>
    public async Task<WorkSpaceTag> AddWorkSpaceTagAsync(WorkSpaceTag workSpaceTag, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.WorkSpaceTags.AddAsync(workSpaceTag, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return workSpaceTag;
    }

    /// <summary>
    /// 删除工作空间-标签关联
    /// </summary>
    public async Task DeleteWorkSpaceTagAsync(long workSpaceId, long tagId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var workSpaceTag = await ctx.WorkSpaceTags
            .FirstOrDefaultAsync(wst => wst.WorkSpaceId == workSpaceId && wst.TagId == tagId, cancellationToken);

        if (workSpaceTag != null)
        {
            ctx.WorkSpaceTags.Remove(workSpaceTag);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }
}
