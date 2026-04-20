using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using Splat;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 项目/工作空间 ViewModel 基类
/// </summary>
public abstract class ItemViewModelBase<TDto> : ReactiveObject where TDto : class
{
    protected TDto _dto;

    public long Id => GetId();

    public string Name => GetName();

    public string? Description => GetDescription();

    protected bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        protected set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }

    public DateTime? FavoritedAt => GetFavoritedAt();

    public DateTime? LastOpenedAt => GetLastOpenedAt();

    /// <summary>
    /// 本地化字符串访问器
    /// </summary>
    protected static LocalizedStrings L => Locator.Current.GetService<LocalizedStrings>()!;

    #region Commands

    /// <summary>
    /// Toggle favorite status command
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleFavoriteCommand { get; protected set; } = null!;

    /// <summary>
    /// Edit command - requests parent to open edit dialog
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; protected set; } = null!;

    /// <summary>
    /// Delete command - requests parent to handle deletion with confirmation
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; protected set; } = null!;

    /// <summary>
    /// Move to folder command - requests parent to show folder selector
    /// </summary>
    public ReactiveCommand<Unit, Unit> MoveToFolderCommand { get; protected set; } = null!;

    #endregion

    protected ItemViewModelBase(TDto dto)
    {
        _dto = dto;
    }

    #region Abstract Methods

    protected abstract long GetId();
    protected abstract string GetName();
    protected abstract string? GetDescription();
    protected abstract DateTime? GetFavoritedAt();
    protected abstract DateTime? GetLastOpenedAt();

    /// <summary>
    /// Send edit request message via MessageBus
    /// </summary>
    protected abstract void SendEditRequest();

    /// <summary>
    /// Send delete request message via MessageBus
    /// </summary>
    protected abstract void SendDeleteRequest();

    /// <summary>
    /// Send favorite changed message via MessageBus
    /// </summary>
    protected abstract void SendFavoriteChanged();

    /// <summary>
    /// Send move to folder request message via MessageBus
    /// </summary>
    protected abstract void SendMoveToFolderRequest();

    /// <summary>
    /// Toggle favorite status
    /// </summary>
    protected abstract Task ToggleFavoriteAsync();

    /// <summary>
    /// Get the underlying DTO for editing
    /// </summary>
    public abstract TDto GetDto();

    /// <summary>
    /// Update the ViewModel after item is edited
    /// </summary>
    public abstract void UpdateFromDto(TDto updatedDto);

    #endregion
}
