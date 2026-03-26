using ProjectHub.Application.Interfaces;
using ReactiveUI;

namespace ProjectHub.UI.ViewModels;

/// <summary>
/// Group ViewModel
/// </summary>
public partial class GroupViewModel : ReactiveObject
{
    private readonly GroupDto _groupDto;

    public long Id => _groupDto.Id;
    
    public string Name => _groupDto.Name;
    
    public string? Description => _groupDto.Description;
    
    public int ProjectCount => _groupDto.ProjectCount;
    
    public bool IsExpanded => _groupDto.IsExpanded;
    
    public int SortOrder => _groupDto.SortOrder;

    public GroupViewModel(GroupDto groupDto)
    {
        _groupDto = groupDto;
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
    
    public string? Color => _tagDto.Color;
    
    public int ProjectCount => _tagDto.ProjectCount;

    public TagViewModel(TagDto tagDto)
    {
        _tagDto = tagDto;
    }
}
