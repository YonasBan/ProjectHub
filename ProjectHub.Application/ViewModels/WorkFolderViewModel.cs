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

    public bool IsExpanded => _workFolderDto.IsExpanded;

    public int SortOrder => _workFolderDto.SortOrder;

    public long? ParentId => _workFolderDto.ParentId;

    public WorkFolderViewModel(WorkFolderDto workFolderDto)
    {
        _workFolderDto = workFolderDto;
    }
}
