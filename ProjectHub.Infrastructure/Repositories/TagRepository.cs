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

    public async Task<IReadOnlyList<Tag>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        // 查询标签
        var tags = await _dbContext.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // 为每个标签计算项目数量
        foreach (var tag in tags)
        {
            var projectCount = await _dbContext.ProjectTags
                .CountAsync(pt => pt.TagId == tag.Id, cancellationToken);
            
            // 使用领域方法设置 ProjectCount
            tag.UpdateProjectCount(projectCount);
        }

        return tags;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AnyAsync(t => t.Name == name, cancellationToken);
    }
}
