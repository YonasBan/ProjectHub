using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;

namespace ProjectHub.Infrastructure.Repositories;

/// <summary>
/// 分组仓储实现
/// </summary>
public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _dbContext;

    public GroupRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Group?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Groups.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Groups.ToListAsync(cancellationToken);
    }

    public async Task<Group> AddAsync(Group entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Groups.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Group entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Groups.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Group entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Groups.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Group>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        // 查询分组
        var groups = await _dbContext.Groups
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(cancellationToken);

        // 为每个分组计算项目数量
        foreach (var group in groups)
        {
            var projectCount = await _dbContext.Projects
                .CountAsync(p => p.GroupId == group.Id && !p.IsArchived, cancellationToken);
            
            // 使用领域方法设置 ProjectCount
            group.UpdateProjectCount(projectCount);
        }

        return groups;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Groups
            .AnyAsync(g => g.Name == name, cancellationToken);
    }
}
