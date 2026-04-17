using ProjectHub.Application.DTOs;
using ReactiveUI;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// WorkFolder ViewModel
/// </summary>
public partial class WorkFolderViewModel : ReactiveObject
{
    private readonly WorkFolderDto _workFolderDto;

    public long Id => _workFolderDto.Id;

    public string Name => _workFolderDto.Name;

    public string? Description => _workFolderDto.Description;

    public int ProjectCount => _workFolderDto.ProjectCount;

    public int WorkSpaceCount => _workFolderDto.WorkSpaceCount;

    /// <summary>
    /// 总数量（项目 + 工作空间）
    /// </summary>
    public int TotalCount => _workFolderDto.ProjectCount + _workFolderDto.WorkSpaceCount;

    public bool IsExpanded => _workFolderDto.IsExpanded;

    public int SortOrder => _workFolderDto.SortOrder;

    public long? ParentId => _workFolderDto.ParentId;

    public WorkFolderViewModel(WorkFolderDto workFolderDto)
    {
        _workFolderDto = workFolderDto;
    }
}
