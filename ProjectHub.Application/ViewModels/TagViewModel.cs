using ProjectHub.Application.DTOs;
using ReactiveUI;

namespace ProjectHub.Application.ViewModels;

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
