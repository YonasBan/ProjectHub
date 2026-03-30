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
    private readonly AppDbContext _dbContext;

    public WorkFolderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkFolder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkFolders.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WorkFolders.ToListAsync(cancellationToken);
    }

    public async Task<WorkFolder> AddAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkFolders.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkFolders.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkFolder entity, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkFolders.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        // 查询工作文件夹（包含关联的项目数量）
        var workFolders = await _dbContext.WorkFolders
            .Select(w => new
            {
                WorkFolder = w,
                ProjectCount = _dbContext.ProjectWorkFolders
                    .Count(pwf => pwf.WorkFolderId == w.Id && 
                                  !_dbContext.Projects.Any(p => p.Id == pwf.ProjectId))
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
        return await _dbContext.WorkFolders
            .AnyAsync(w => w.Name == name, cancellationToken);
    }
}
