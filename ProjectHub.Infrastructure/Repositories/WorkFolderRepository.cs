using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 工作文件夹仓储实现
/// </summary>
public class WorkFolderRepository : IWorkFolderRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public WorkFolderRepository(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<WorkFolder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        return await ctx.WorkFolders.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        return await ctx.WorkFolders.ToListAsync(cancellationToken);
    }

    public async Task<WorkFolder> AddAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        await ctx.WorkFolders.AddAsync(entity, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        ctx.WorkFolders.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        ctx.WorkFolders.Remove(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        // 查询工作文件夹（包含关联的项目数量）
        var workFolders = await ctx.WorkFolders
            .Select(w => new
            {
                WorkFolder = w,
                ProjectCount = ctx.ProjectWorkFolders
                    .Count(pwf => pwf.WorkFolderId == w.Id &&
                                  !ctx.Projects.Any(p => p.Id == pwf.ProjectId))
            })
            .OrderBy(x => x.WorkFolder.SortOrder)
            .ThenBy(x => x.WorkFolder.Name)
            .ToListAsync(cancellationToken);

        // 设置项目数量
        foreach (var item in workFolders)
        {
            item.WorkFolder.UpdateProjectCount(item.ProjectCount);
        }

        return workFolders.Select(x => x.WorkFolder).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        return await ctx.WorkFolders
            .AnyAsync(w => w.Name == name, cancellationToken);
    }
}