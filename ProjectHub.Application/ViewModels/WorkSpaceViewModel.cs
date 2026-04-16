using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using Splat;
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
    /// 本地化字符串访问器
    /// </summary>
    private static LocalizedStrings L => Locator.Current.GetService<LocalizedStrings>()!;
    
    /// <summary>
    /// 项目数量显示文本（支持本地化）
    /// </summary>
    public string ProjectCountDisplay => string.Format(L.WorkSpace_ProjectCount, ProjectCount);
    
    /// <summary>
    /// 上次打开时间显示文本（支持本地化）
    /// </summary>
    public string LastOpenedDisplay => LastOpenedAt.HasValue 
        ? string.Format(L.WorkSpace_LastOpened, LastOpenedAt.Value) 
        : "";

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
    /// Send edit request message via MessageBus
    /// </summary>
    private void SendEditRequest() => MessageBus.Current.SendMessage(new WorkSpaceEditRequestMessage(this));

    /// <summary>
    /// Send delete request message via MessageBus
    /// </summary>
    private void SendDeleteRequest() => MessageBus.Current.SendMessage(new WorkSpaceDeleteRequestMessage(this));

    /// <summary>
    /// Send favorite changed message via MessageBus
    /// </summary>
    private void SendFavoriteChanged() => MessageBus.Current.SendMessage(new WorkSpaceFavoriteChangedMessage(this));

    public WorkSpaceViewModel(WorkSpaceDto workSpaceDto, IWorkSpaceAppService? workSpaceAppService = null)
    {
        _workSpaceDto = workSpaceDto;
        _workSpaceAppService = workSpaceAppService;
        _isFavorite = workSpaceDto.IsFavorite;

        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(SendEditRequest);
        DeleteCommand = ReactiveCommand.Create(SendDeleteRequest);
        LaunchAllCommand = ReactiveCommand.CreateFromTask(LaunchAllAsync);
        
        // 订阅语言变化，刷新本地化显示属性
        L?.CultureChanged.Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(ProjectCountDisplay));
            this.RaisePropertyChanged(nameof(LastOpenedDisplay));
        });
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
        
        // 通过 MessageBus 通知收藏状态变化
        SendFavoriteChanged();
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
    /// Launch all projects in the workspace
    /// </summary>
    private async Task LaunchAllAsync()
    {
        if (_workSpaceAppService == null) return;

        await _workSpaceAppService.LaunchAllAsync(Id);
    }
}

// ========== MessageBus Messages ==========

/// <summary>
/// 工作空间编辑请求消息
/// </summary>
public record WorkSpaceEditRequestMessage(WorkSpaceViewModel WorkSpace);

/// <summary>
/// 工作空间删除请求消息
/// </summary>
public record WorkSpaceDeleteRequestMessage(WorkSpaceViewModel WorkSpace);

/// <summary>
/// 工作空间收藏状态变化消息
/// </summary>
public record WorkSpaceFavoriteChangedMessage(WorkSpaceViewModel WorkSpace);
