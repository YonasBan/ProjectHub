using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using Splat;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 项目/工作空间 ViewModel 公共接口
/// </summary>
public interface IItemViewModel
{
    long Id { get; }
    string Name { get; }
    string? Description { get; }
    bool IsFavorite { get; }
    DateTime? FavoritedAt { get; }
    DateTime? LastOpenedAt { get; }
}

/// <summary>
/// 项目/工作空间 ViewModel 基类
/// </summary>
public abstract class ItemViewModelBase<TDto> : ReactiveObject, IItemViewModel where TDto : class
{
    protected TDto _dto;
    protected readonly IDialogService _dialogService;
    protected readonly IServiceProvider _serviceProvider;

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
    /// Edit command - opens edit dialog directly
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; protected set; } = null!;

    /// <summary>
    /// Delete command - handles deletion with confirmation
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; protected set; } = null!;

    /// <summary>
    /// Move to folder command - shows folder selector
    /// </summary>
    public ReactiveCommand<Unit, Unit> MoveToFolderCommand { get; protected set; } = null!;

    #endregion

   

    protected ItemViewModelBase(TDto dto, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _dto = dto;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
    }

    #region Abstract Methods

    protected abstract long GetId();
    protected abstract string GetName();
    protected abstract string? GetDescription();
    protected abstract DateTime? GetFavoritedAt();
    protected abstract DateTime? GetLastOpenedAt();

    /// <summary>
    /// Open edit dialog and handle editing
    /// </summary>
    protected abstract Task EditAsync();

    /// <summary>
    /// Handle deletion with confirmation
    /// </summary>
    protected abstract Task DeleteAsync();

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

    /// <summary>
    /// Get the delete confirmation title resource key
    /// </summary>
    protected abstract string GetDeleteConfirmTitle();

    /// <summary>
    /// Get the delete confirmation message resource key
    /// </summary>
    protected abstract string GetDeleteConfirmMessage();

    /// <summary>
    /// Get the delete success message resource key
    /// </summary>
    protected abstract string GetDeleteSuccessMessage();

    /// <summary>
    /// Get the delete failed message resource key
    /// </summary>
    protected abstract string GetDeleteFailedMessage();

    /// <summary>
    /// Get the save failed message resource key
    /// </summary>
    protected abstract string GetSaveFailedMessage();

    #endregion
}
