using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
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
    
    public bool IsExpanded => _workFolderDto.IsExpanded;
    
    public int SortOrder => _workFolderDto.SortOrder;

    public long? ParentId => _workFolderDto.ParentId;

    public WorkFolderViewModel(WorkFolderDto workFolderDto)
    {
        _workFolderDto = workFolderDto;
    }
}

/// <summary>
/// WorkSpace ViewModel
/// </summary>
public partial class WorkSpaceViewModel : ReactiveObject
{
    private readonly WorkSpaceDto _workSpaceDto;

    public long Id => _workSpaceDto.Id;
    
    public string Name => _workSpaceDto.Name;
    
    public string? Description => _workSpaceDto.Description;
    
    public int ProjectCount => _workSpaceDto.ProjectCount;
    
    public int SortOrder => _workSpaceDto.SortOrder;
    
    public bool IsFavorite => _workSpaceDto.IsFavorite;
    
    public DateTime? FavoritedAt => _workSpaceDto.FavoritedAt;
    
    public DateTime? LastOpenedAt => _workSpaceDto.LastOpenedAt;

    public WorkSpaceViewModel(WorkSpaceDto workSpaceDto)
    {
        _workSpaceDto = workSpaceDto;
    }
}

/// <summary>
/// Tag ViewModel
/// </summary>
public partial class TagViewModel : ReactiveObject
{
    private readonly TagDto _tagDto;

    public long Id => _tagDto.Id;

    public string Name => _tagDto.Name;

    public string Color => _tagDto.Color;

    public string? Description => _tagDto.Description;

    public int SortOrder => _tagDto.SortOrder;

    public int ProjectCount => _tagDto.ProjectCount;

    public int WorkSpaceCount => _tagDto.WorkSpaceCount;

    public DateTime CreatedAt => _tagDto.CreatedAt;

    public DateTime? UpdatedAt => _tagDto.UpdatedAt;

    public TagViewModel(TagDto tagDto)
    {
        _tagDto = tagDto;
    }
}
