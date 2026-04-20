using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels.DialogViewModel;
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

    public WorkSpaceViewModel(
        WorkSpaceDto workSpaceDto,
        IDialogService dialogService,
        IServiceProvider serviceProvider,
        IWorkSpaceAppService? workSpaceAppService = null)
        : base(workSpaceDto, dialogService, serviceProvider)
    {
        _workSpaceAppService = workSpaceAppService;
        _isFavorite = workSpaceDto.IsFavorite;

        ToggleFavoriteCommand = ReactiveCommand.CreateFromTask(ToggleFavoriteAsync);
        EditCommand = ReactiveCommand.CreateFromTask(EditAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync);
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

    protected override async Task EditAsync()
    {
        try
        {
            var dialogViewModel = _serviceProvider.GetRequiredService<WorkSpaceDialogViewModel>();
            await dialogViewModel.InitializeForEditAsync(Id);

            var result = await _dialogService.ShowDialogAsync<WorkSpaceDialogViewModel, WorkSpaceDto?>(dialogViewModel);

            if (result.Confirmed && result.Value != null)
            {
                UpdateProjectCount(result.Value.ProjectCount);
                _dialogService.ShowNotification(
                    string.Format(L.Message_WorkSpaceUpdated, Name),
                    NotificationType.Success,
                    3000);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(L.Message_SaveFailed, ex.Message);
        }
    }

    protected override async Task DeleteAsync()
    {
        try
        {
            var confirmed = await _dialogService.ShowConfirmAsync(
                L.DeleteConfirm_Title,
                string.Format(L.DeleteConfirm_Message, Name));

            if (!confirmed) return;

            if (_workSpaceAppService == null) return;

            await _workSpaceAppService.DeleteAsync(Id);

            _dialogService.ShowNotification(
                string.Format(L.Message_WorkSpaceDeleted, Name),
                NotificationType.Success,
                3000);

            // 通知父级刷新
            OnItemChanged(ItemChangedType.Deleted);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(L.Message_DeleteFailed, ex.Message);
        }
    }

    protected override void SendFavoriteChanged() => OnItemChanged(ItemChangedType.FavoriteChanged);

    protected override void SendMoveToFolderRequest() => OnItemChanged(ItemChangedType.MovedToFolder);

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

    protected override string GetDeleteConfirmTitle() => L.DeleteConfirm_Title;
    protected override string GetDeleteConfirmMessage() => string.Format(L.DeleteConfirm_Message, Name);
    protected override string GetDeleteSuccessMessage() => string.Format(L.Message_WorkSpaceDeleted, Name);
    protected override string GetDeleteFailedMessage() => L.Message_DeleteFailed;
    protected override string GetSaveFailedMessage() => L.Message_SaveFailed;

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


