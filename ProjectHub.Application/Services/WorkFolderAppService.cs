using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.Services;

/// <summary>
/// 工作文件夹应用服务实现
/// </summary>
public class WorkFolderAppService : IWorkFolderAppService
{
    private readonly IWorkFolderRepository _workFolderRepository;

    public WorkFolderAppService(IWorkFolderRepository workFolderRepository)
    {
        _workFolderRepository = workFolderRepository;
    }

    public async Task<WorkFolderDto> CreateAsync(CreateWorkFolderDto input, CancellationToken cancellationToken = default)
    {
        // 检查同级目录下是否已存在同名文件夹
        if (await _workFolderRepository.ExistsByNameAndParentIdAsync(input.Name, input.ParentId, cancellationToken))
        {
            throw new InvalidOperationException($"同级目录下已存在名称为 '{input.Name}' 的文件夹");
        }

        var workFolder = Domain.Entities.WorkFolder.Create(
            input.Name, 
            input.SortOrder, 
            description: null, 
            parentId: input.ParentId);
        await _workFolderRepository.AddAsync(workFolder, cancellationToken);
        return MapToDto(workFolder);
    }

    public async Task<WorkFolderDto> UpdateAsync(UpdateWorkFolderDto input, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(input.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={input.Id}) not found");

        // TODO: 实现更新逻辑
        return MapToDto(workFolder);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={id}) not found");

        await _workFolderRepository.DeleteAsync(workFolder, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkFolderDto>> GetAllWithProjectCountAsync(CancellationToken cancellationToken = default)
    {
        var workFolders = await _workFolderRepository.GetAllWithProjectCountAsync(cancellationToken);
        return workFolders.Select(MapToDto).ToList();
    }

    public async Task MoveProjectToWorkFolderAsync(long projectId, long? workFolderId, CancellationToken cancellationToken = default)
    {
        // TODO: 实现项目移动
        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加项目到文件夹
    /// </summary>
    public async Task AddProjectToFolderAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(workFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={workFolderId}) not found");

        // 检查是否已存在关联
        var exists = await _workFolderRepository.ExistsProjectFolderAssociationAsync(projectId, workFolderId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("该项目已在所选文件夹中");
        }

        // 获取当前最大排序值
        var maxSortOrder = await _workFolderRepository.GetMaxProjectSortOrderAsync(workFolderId, cancellationToken);

        // 创建关联
        var association = ProjectWorkFolder.Create(projectId, workFolderId, maxSortOrder + 1);
        await _workFolderRepository.AddProjectAssociationAsync(association, cancellationToken);

        // 更新项目计数
        workFolder.UpdateProjectCount(workFolder.ProjectCount + 1);
        await _workFolderRepository.UpdateAsync(workFolder, cancellationToken);
    }

    /// <summary>
    /// 从文件夹移除项目
    /// </summary>
    public async Task RemoveProjectFromFolderAsync(long projectId, long workFolderId, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(workFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={workFolderId}) not found");

        await _workFolderRepository.RemoveProjectAssociationAsync(projectId, workFolderId, cancellationToken);

        // 更新项目计数
        workFolder.UpdateProjectCount(Math.Max(0, workFolder.ProjectCount - 1));
        await _workFolderRepository.UpdateAsync(workFolder, cancellationToken);
    }

    /// <summary>
    /// 添加工作空间到文件夹
    /// </summary>
    public async Task AddWorkSpaceToFolderAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(workFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={workFolderId}) not found");

        // 检查是否已存在关联
        var exists = await _workFolderRepository.ExistsWorkSpaceFolderAssociationAsync(workSpaceId, workFolderId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("该工作空间已在所选文件夹中");
        }

        // 获取当前最大排序值
        var maxSortOrder = await _workFolderRepository.GetMaxWorkSpaceSortOrderAsync(workFolderId, cancellationToken);

        // 创建关联
        var association = WorkSpaceWorkFolder.Create(workSpaceId, workFolderId, maxSortOrder + 1);
        await _workFolderRepository.AddWorkSpaceAssociationAsync(association, cancellationToken);

        // 更新项目计数
        workFolder.UpdateProjectCount(workFolder.ProjectCount + 1);
        await _workFolderRepository.UpdateAsync(workFolder, cancellationToken);
    }

    /// <summary>
    /// 从文件夹移除工作空间
    /// </summary>
    public async Task RemoveWorkSpaceFromFolderAsync(long workSpaceId, long workFolderId, CancellationToken cancellationToken = default)
    {
        var workFolder = await _workFolderRepository.GetByIdAsync(workFolderId, cancellationToken)
            ?? throw new KeyNotFoundException($"WorkFolder (Id={workFolderId}) not found");

        await _workFolderRepository.RemoveWorkSpaceAssociationAsync(workSpaceId, workFolderId, cancellationToken);

        // 更新项目计数
        workFolder.UpdateProjectCount(Math.Max(0, workFolder.ProjectCount - 1));
        await _workFolderRepository.UpdateAsync(workFolder, cancellationToken);
    }

    private static WorkFolderDto MapToDto(Domain.Entities.WorkFolder workFolder)
    {
        return new WorkFolderDto
        {
            Id = workFolder.Id,
            Name = workFolder.Name,
            ParentId = workFolder.ParentId,
            SortOrder = workFolder.SortOrder,
            ProjectCount = workFolder.ProjectCount,
            CreatedAt = workFolder.CreatedAt,
            UpdatedAt = workFolder.UpdatedAt
        };
    }
}
