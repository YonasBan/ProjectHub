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
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProjectRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<Project?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Projects
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Projects
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project> AddAsync(Project entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.Projects.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Project entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        ctx.Projects.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        entity.SoftDelete();
        ctx.Projects.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByWorkFolderIdAsync(long? workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        if (workFolderId == null)
        {
            // 查询未归属工作文件夹的项目
            var allProjectIds = await ctx.Projects
                .Where(p => !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            
            var projectInFolderIds = await ctx.ProjectWorkFolders
                .Select(pwf => pwf.ProjectId)
                .ToListAsync(cancellationToken);
            
            var notInFolderIds = allProjectIds.Except(projectInFolderIds).ToList();
            
            return await ctx.Projects
                .Where(p => !p.IsDeleted && notInFolderIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        // 通过关联表查询属于该工作文件夹的项目
        var projectIds = await ctx.ProjectWorkFolders
            .Where(pwf => pwf.WorkFolderId == workFolderId)
            .Select(pwf => pwf.ProjectId)
            .ToListAsync(cancellationToken);

        return await ctx.Projects
            .Where(p => !p.IsDeleted && projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByWorkSpaceIdAsync(long? workSpaceId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        if (workSpaceId == null)
        {
            // 查询未归属工作空间的项目
            var allProjectIds = await ctx.Projects
                .Where(p => !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            
            var projectInSpaceIds = await ctx.ProjectWorkSpaces
                .Select(pws => pws.ProjectId)
                .ToListAsync(cancellationToken);
            
            var notInSpaceIds = allProjectIds.Except(projectInSpaceIds).ToList();
            
            return await ctx.Projects
                .Where(p => !p.IsDeleted && notInSpaceIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        // 通过关联表查询属于该工作空间的项目
        var projectIds = await ctx.ProjectWorkSpaces
            .Where(pws => pws.WorkSpaceId == workSpaceId)
            .Select(pws => pws.ProjectId)
            .ToListAsync(cancellationToken);

        return await ctx.Projects
            .Where(p => !p.IsDeleted && projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetRecentlyUsedAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Projects
            .Where(p => !p.IsDeleted && p.LastOpenedAt != null)
            .OrderByDescending(p => p.LastOpenedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetFavoriteProjectsAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Projects
            .Where(p => !p.IsDeleted && p.IsFavorite)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.Projects
            .Where(p => !p.IsDeleted)
            .AnyAsync(p => p.Path == path, cancellationToken);
    }
}
