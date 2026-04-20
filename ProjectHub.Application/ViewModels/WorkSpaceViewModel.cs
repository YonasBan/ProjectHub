using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ReactiveUI;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// WorkSpace ViewModel
/// </summary>
public partial class WorkSpaceViewModel : ItemViewModelBase<WorkSpaceDto>
{
    private readonly IWorkSpaceAppService? _workSpaceAppService;

    public int ProjectCount => ((WorkSpaceDto)_dto).ProjectCount;

    public int SortOrder => ((WorkSpaceDto)_dto).SortOrder;

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
    /// Launch all projects in workspace command
    /// </summary>
    public ReactiveCommand<Unit, Unit> LaunchAllCommand { get; }

    public WorkSpaceViewModel(WorkSpaceDto workSpaceDto, IWorkSpaceAppService? workSpaceAppService = null)
        : base(workSpaceDto)
    {
        _workSpaceAppService = workSpaceAppService;
        _isFavorite = workSpaceDto.IsFavorite;

        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.Create(SendEditRequest);
        DeleteCommand = ReactiveCommand.Create(SendDeleteRequest);
        LaunchAllCommand = ReactiveCommand.CreateFromTask(LaunchAllAsync);
        MoveToFolderCommand = ReactiveCommand.Create(SendMoveToFolderRequest);

        // 订阅语言变化，刷新本地化显示属性
        L?.CultureChanged.Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(ProjectCountDisplay));
            this.RaisePropertyChanged(nameof(LastOpenedDisplay));
        });
    }

    #region Base Class Implementations

    protected override long GetId() => ((WorkSpaceDto)_dto).Id;

    protected override string GetName() => ((WorkSpaceDto)_dto).Name;

    protected override string? GetDescription() => ((WorkSpaceDto)_dto).Description;

    protected override DateTime? GetFavoritedAt() => ((WorkSpaceDto)_dto).FavoritedAt;

    protected override DateTime? GetLastOpenedAt() => ((WorkSpaceDto)_dto).LastOpenedAt;

    protected override void SendEditRequest() => MessageBus.Current.SendMessage(new WorkSpaceEditRequestMessage(this));

    protected override void SendDeleteRequest() => MessageBus.Current.SendMessage(new WorkSpaceDeleteRequestMessage(this));

    protected override void SendFavoriteChanged() => MessageBus.Current.SendMessage(new WorkSpaceFavoriteChangedMessage(this));

    protected override void SendMoveToFolderRequest() => MessageBus.Current.SendMessage(new WorkSpaceMoveToFolderRequestMessage(this));

    public override WorkSpaceDto GetDto() => (WorkSpaceDto)_dto;

    public override void UpdateFromDto(WorkSpaceDto updatedDto)
    {
        _dto = updatedDto;
        this.RaisePropertyChanged(string.Empty);
    }

    protected override async Task ToggleFavoriteAsync()
    {
        if (_workSpaceAppService == null) return;

        var newFavoriteStatus = !IsFavorite;
        await _workSpaceAppService.SetFavoriteAsync(Id, newFavoriteStatus);
        IsFavorite = newFavoriteStatus;

        SendFavoriteChanged();
    }

    #endregion

    /// <summary>
    /// Launch all projects in the workspace
    /// </summary>
    private async Task LaunchAllAsync()
    {
        if (_workSpaceAppService == null) return;

        await _workSpaceAppService.LaunchAllAsync(Id);
    }

    internal void UpdateProjectCount(int selectedProjectCount)
    {
        ((WorkSpaceDto)_dto).ProjectCount = selectedProjectCount;
        this.RaisePropertyChanged(nameof(ProjectCountDisplay));
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

/// <summary>
/// 工作空间移动到文件夹请求消息
/// </summary>
public record WorkSpaceMoveToFolderRequestMessage(WorkSpaceViewModel WorkSpace);
