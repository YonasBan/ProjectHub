using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 项目与工作空间关联仓储实现
/// </summary>
public class ProjectWorkSpaceRepository : IProjectWorkSpaceRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProjectWorkSpaceRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task AddProjectToWorkSpaceAsync(long projectId, long workSpaceId, bool isEnabled = true, int sortOrder = 0, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        // 检查是否已存在
        var exists = await ctx.ProjectWorkSpaces
            .AnyAsync(pws => pws.ProjectId == projectId && pws.WorkSpaceId == workSpaceId, cancellationToken);
        
        if (exists)
        {
            // 已存在则更新启用状态和排序
            var existing = await ctx.ProjectWorkSpaces
                .FirstAsync(pws => pws.ProjectId == projectId && pws.WorkSpaceId == workSpaceId, cancellationToken);
            existing.SetEnabled(isEnabled);
            existing.UpdateSortOrder(sortOrder);
        }
        else
        {
            // 创建新的关联
            var projectWorkSpace = new ProjectWorkSpace(projectId, workSpaceId, sortOrder, isEnabled);
            await ctx.ProjectWorkSpaces.AddAsync(projectWorkSpace, cancellationToken);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveProjectFromWorkSpaceAsync(long projectId, long workSpaceId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        var projectWorkSpace = await ctx.ProjectWorkSpaces
            .FirstOrDefaultAsync(pws => pws.ProjectId == projectId && pws.WorkSpaceId == workSpaceId, cancellationToken);
        
        if (projectWorkSpace != null)
        {
            ctx.ProjectWorkSpaces.Remove(projectWorkSpace);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetProjectEnabledAsync(long projectId, long workSpaceId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        var projectWorkSpace = await ctx.ProjectWorkSpaces
            .FirstOrDefaultAsync(pws => pws.ProjectId == projectId && pws.WorkSpaceId == workSpaceId, cancellationToken);
        
        if (projectWorkSpace != null)
        {
            projectWorkSpace.SetEnabled(isEnabled);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ProjectWorkSpace>> GetByWorkSpaceIdAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        return await ctx.ProjectWorkSpaces
            .Where(pws => pws.WorkSpaceId == workSpaceId)
            .OrderBy(pws => pws.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAllProjectsFromWorkSpaceAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        var projectWorkSpaces = await ctx.ProjectWorkSpaces
            .Where(pws => pws.WorkSpaceId == workSpaceId)
            .ToListAsync(cancellationToken);
        
        ctx.ProjectWorkSpaces.RemoveRange(projectWorkSpaces);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}
