using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 工作空间仓储实现
/// </summary>
public class WorkSpaceRepository : IWorkSpaceRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public WorkSpaceRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<WorkSpace?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkSpaces
            .Where(w => !w.IsDeleted)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkSpaces
            .Where(w => !w.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkSpace> AddAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.WorkSpaces.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        ctx.WorkSpaces.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        entity.SoftDelete();
        ctx.WorkSpaces.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        
        // 查询工作空间（包含关联的项目数量）
        var workSpaces = await ctx.WorkSpaces
            .Where(w => !w.IsDeleted)
            .Select(w => new
            {
                WorkSpace = w,
                ProjectCount = ctx.ProjectWorkSpaces
                    .Count(pws => pws.WorkSpaceId == w.Id && 
                                  !ctx.Projects.Any(p => p.Id == pws.ProjectId && !p.IsDeleted))
            })
            .OrderBy(x => x.WorkSpace.SortOrder)
            .ThenBy(x => x.WorkSpace.Name)
            .ToListAsync(cancellationToken);

        // 设置项目数量
        foreach (var item in workSpaces)
        {
            item.WorkSpace.UpdateProjectCount(item.ProjectCount);
        }

        return workSpaces.Select(x => x.WorkSpace).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkSpaces
            .Where(w => !w.IsDeleted)
            .AnyAsync(w => w.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetRecentlyOpenedAsync(int count, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        // 按最近打开时间降序排序，获取指定数量的工作空间
        return await ctx.WorkSpaces
            .Where(w => !w.IsDeleted && w.LastOpenedAt.HasValue)
            .OrderByDescending(w => w.LastOpenedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
