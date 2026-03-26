using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 项目仓储实现
/// 使用 EF Core 访问 SQLite 数据库
/// </summary>
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects.ToListAsync(cancellationToken);
    }

    public async Task<Project> AddAsync(Project entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Project entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByGroupIdAsync(long? groupId, CancellationToken cancellationToken = default)
    {
        if (groupId == null)
        {
            // 查询未分组的项目
            return await _dbContext.Projects
                .Where(p => p.GroupId == null && !p.IsArchived)
                .ToListAsync(cancellationToken);
        }

        return await _dbContext.Projects
            .Where(p => p.GroupId == groupId && !p.IsArchived)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByTagIdAsync(long tagId, CancellationToken cancellationToken = default)
    {
        var projectIds = await _dbContext.ProjectTags
            .Where(pt => pt.TagId == tagId)
            .Select(pt => pt.ProjectId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Projects
            .Where(p => projectIds.Contains(p.Id) && !p.IsArchived)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => !p.IsArchived && 
                       (p.Name.Contains(keyword) || 
                        p.Path.Contains(keyword) || 
                        (p.Description != null && p.Description.Contains(keyword))))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => !p.IsArchived && p.LastOpenedAt != null)
            .OrderByDescending(p => p.LastOpenedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetArchivedProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => p.IsArchived)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetFavoriteProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => !p.IsArchived && p.IsFavorite)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AnyAsync(p => p.Path == path, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByTypeAsync(ProjectType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Where(p => p.Type == type && !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
