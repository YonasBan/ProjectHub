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
    private readonly AppDbContext _dbContext;

    public WorkSpaceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkSpace?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkSpaces.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkSpaces.ToListAsync(cancellationToken);
    }

    public async Task<WorkSpace> AddAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkSpaces.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkSpaces.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkSpace entity, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkSpaces.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        // 查询工作空间（包含关联的项目数量）
        var workSpaces = await _dbContext.WorkSpaces
            .Select(w => new
            {
                WorkSpace = w,
                ProjectCount = _dbContext.ProjectWorkSpaces
                    .Count(pws => pws.WorkSpaceId == w.Id && 
                                  !_dbContext.Projects.Any(p => p.Id == pws.ProjectId && p.IsArchived))
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
        return await _dbContext.WorkSpaces
            .AnyAsync(w => w.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkSpace>> GetRecentlyOpenedAsync(int count, CancellationToken cancellationToken = default)
    {
        // 按最近打开时间降序排序，获取指定数量的工作空间
        return await _dbContext.WorkSpaces
            .Where(w => w.LastOpenedAt.HasValue)
            .OrderByDescending(w => w.LastOpenedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
