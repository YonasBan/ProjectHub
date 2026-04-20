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
        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted)
            .ToListAsync(cancellationToken);
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
        entity.SoftDelete();
        ctx.WorkFolders.Update(entity);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        // 查询工作文件夹（包含关联的项目和工作空间数量）
        var workFolders = await ctx.WorkFolders
            .Where(w => !w.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
        return workFolders.ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted)
            .AnyAsync(w => w.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndParentIdAsync(string name, long? parentId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();

        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted)
            .AnyAsync(w => w.Name == name && w.ParentId == parentId, cancellationToken);
    }

    public async Task<bool> ExistsProjectFolderAssociationAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.ProjectWorkFolders
            .AnyAsync(pwf => pwf.ProjectId == projectId && pwf.WorkFolderId == workFolderId, cancellationToken);
    }

    public async Task<bool> ExistsWorkSpaceFolderAssociationAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkSpaceWorkFolders
            .AnyAsync(wwf => wwf.WorkSpaceId == workSpaceId && wwf.WorkFolderId == workFolderId, cancellationToken);
    }

    public async Task<int> GetMaxProjectSortOrderAsync(long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var maxOrder = await ctx.ProjectWorkFolders
            .Where(pwf => pwf.WorkFolderId == workFolderId)
            .MaxAsync(pwf => (int?)pwf.SortOrder, cancellationToken);
        return maxOrder ?? 0;
    }

    public async Task<int> GetMaxWorkSpaceSortOrderAsync(long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var maxOrder = await ctx.WorkSpaceWorkFolders
            .Where(wwf => wwf.WorkFolderId == workFolderId)
            .MaxAsync(wwf => (int?)wwf.SortOrder, cancellationToken);
        return maxOrder ?? 0;
    }

    public async Task AddProjectAssociationAsync(ProjectWorkFolder association, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.ProjectWorkFolders.AddAsync(association, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveProjectAssociationAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var association = await ctx.ProjectWorkFolders
            .FirstOrDefaultAsync(pwf => pwf.ProjectId == projectId && pwf.WorkFolderId == workFolderId, cancellationToken);
        if (association != null)
        {
            ctx.ProjectWorkFolders.Remove(association);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddWorkSpaceAssociationAsync(WorkSpaceWorkFolder association, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        await ctx.WorkSpaceWorkFolders.AddAsync(association, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveWorkSpaceAssociationAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        var association = await ctx.WorkSpaceWorkFolders
            .FirstOrDefaultAsync(wwf => wwf.WorkSpaceId == workSpaceId && wwf.WorkFolderId == workFolderId, cancellationToken);
        if (association != null)
        {
            ctx.WorkSpaceWorkFolders.Remove(association);
            await ctx.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<WorkFolder>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted && w.ParentId == parentId)
            .OrderBy(w => w.SortOrder)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolder>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.CreateDbContext();
        return await ctx.WorkFolders
            .Where(w => !w.IsDeleted && (w.ParentId == null || w.ParentId == 0))
            .OrderBy(w => w.SortOrder)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}