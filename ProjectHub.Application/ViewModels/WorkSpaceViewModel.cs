using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ReactiveUI;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// WorkSpace ViewModel
/// </summary>
public partial class WorkSpaceViewModel : ReactiveObject
{
    private WorkSpaceDto _workSpaceDto;
    private readonly IWorkSpaceAppService? _workSpaceAppService;

    public long Id => _workSpaceDto.Id;

    public string Name => _workSpaceDto.Name;

    public string? Description => _workSpaceDto.Description;

    public int ProjectCount => _workSpaceDto.ProjectCount;

    public int SortOrder => _workSpaceDto.SortOrder;

    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        private set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }

    public DateTime? FavoritedAt => _workSpaceDto.FavoritedAt;

    public DateTime? LastOpenedAt => _workSpaceDto.LastOpenedAt;

    /// <summary>
    /// Toggle favorite status command
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; }

    /// <summary>
    /// Edit workspace command - requests parent to open edit dialog
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; }

    /// <summary>
    /// Delete workspace command - requests parent to handle deletion with confirmation
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    /// <summary>
    /// Launch all projects in workspace command
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchAllCommand { get; }

    /// <summary>
    /// Event raised when edit is requested
    /// </summary>
    public event EventHandler<WorkSpaceViewModel>? EditRequested;

    /// <summary>
    /// Event raised when delete is requested
    /// </summary>
    public event EventHandler<WorkSpaceViewModel>? DeleteRequested;

    /// <summary>
    /// Event raised when favorite status changed
    /// </summary>
    public event EventHandler<WorkSpaceViewModel>? FavoriteChanged;

    public WorkSpaceViewModel(WorkSpaceDto workSpaceDto, IWorkSpaceAppService? workSpaceAppService = null)
    {
        _workSpaceDto = workSpaceDto;
        _workSpaceAppService = workSpaceAppService;
        _isFavorite = workSpaceDto.IsFavorite;

        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(RequestEdit);
        DeleteCommand = ReactiveCommand.Create(RequestDelete);
        LaunchAllCommand = ReactiveCommand.CreateFromTask(LaunchAllAsync);
    }

    /// <summary>
    /// Get the underlying DTO for editing
    /// </summary>
    public WorkSpaceDto GetWorkSpaceDto() => _workSpaceDto;

    /// <summary>
    /// Toggle favorite status
    /// </summary>
    private async Task ToggleFavoriteAsync()
    {
        if (_workSpaceAppService == null) return;

        var newFavoriteStatus = !IsFavorite;
        await _workSpaceAppService.SetFavoriteAsync(Id, newFavoriteStatus);
        IsFavorite = newFavoriteStatus;
        
        // 通知收藏状态变化
        FavoriteChanged?.Invoke(this, this);
    }

    /// <summary>
    /// Request edit - raises event for parent ViewModel to handle
    /// </summary>
    private void RequestEdit()
    {
        EditRequested?.Invoke(this, this);
    }

    /// <summary>
    /// Update the ViewModel after workspace is edited
    /// </summary>
    public void UpdateFromDto(WorkSpaceDto updatedDto)
    {
        _workSpaceDto = updatedDto;
        this.RaisePropertyChanged(string.Empty); // Notify all properties changed
    }

    /// <summary>
    /// Request delete - raises event for parent ViewModel to handle with confirmation
    /// </summary>
    private void RequestDelete()
    {
        DeleteRequested?.Invoke(this, this);
    }

    /// <summary>
    /// Launch all projects in the workspace
    /// </summary>
    private async Task LaunchAllAsync()
    {
        if (_workSpaceAppService == null) return;

        await _workSpaceAppService.LaunchAllAsync(Id);
    }
}
