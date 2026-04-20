using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels.DialogViewModel;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 内容区域 ViewModel - 管理右侧内容显示和交互
/// </summary>
public class ContentViewModel : ViewModelBase
{
    #region 注入服务

    private readonly IProjectAppService _projectAppService;
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;

    #endregion

    #region 数据集合属性

    /// <summary>
    /// 内容项列表（统一显示项目和工作空间）
    /// </summary>
    private ObservableCollection<object>? _contentItems;

    public ObservableCollection<object> ContentItems
    {
        get => _contentItems ??= new();
        set => this.RaiseAndSetIfChanged(ref _contentItems, value);
    }

    #endregion

    #region 引用属性

    /// <summary>
    /// 侧边栏 ViewModel 引用
    /// </summary>
    public SidebarViewModel SidebarViewModel { get; }

    #endregion

    #region Reactive Commands

    /// <summary>
    /// 添加内容命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddContentCommand { get; }

    #endregion

    #region 构造函数

    public ContentViewModel(
        ILogger<ContentViewModel> logger,
        IProjectAppService projectAppService,
        IWorkSpaceAppService workSpaceAppService,
        IWorkFolderAppService workFolderAppService,
        IServiceProvider serviceProvider,
        IDialogService dialogService,
        IScheduler mainThreadScheduler,
        SidebarViewModel sidebarViewModel)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workSpaceAppService = workSpaceAppService;
        _workFolderAppService = workFolderAppService;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        SidebarViewModel = sidebarViewModel;

        AddContentCommand = ReactiveCommand.CreateFromTask(AddContentAsync);

        // 订阅侧边栏选中项变化，自动更新内容
        SidebarViewModel.SelectedItemChanged += OnSidebarSelectedItemChanged;
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 根据选中的树节点更新内容项
    /// </summary>
    public async void UpdateContentItems(TreeItemViewModel selectedItem)
    {
        ContentItems.Clear();
        switch (selectedItem.ItemType)
        {
            case TreeItemType.AllProjects:
                // 显示所有项目
                foreach (var project in SidebarViewModel.Projects)
                {
                    ContentItems.Add(project);
                }
                break;

            case TreeItemType.RecentProject:
                // 显示最近使用的项目和工作空间（混合按最后打开时间排序，最多20个）
                var recentItems = SidebarViewModel.Projects
                    .Where(p => p.LastOpenedAt.HasValue)
                    .Select(p => (object)p)
                    .Concat(SidebarViewModel.WorkSpaces.Where(w => w.LastOpenedAt.HasValue).Select(w => (object)w))
                    .OrderByDescending(item => item is ProjectViewModel p ? p.LastOpenedAt : ((WorkSpaceViewModel)item).LastOpenedAt)
                    .Take(20)
                    .ToList();
                foreach (var item in recentItems)
                {
                    ContentItems.Add(item);
                }
                break;

            case TreeItemType.FavoriteProject:
                // 显示收藏的项目和工作空间（混合按收藏时间排序）
                var favoriteItems = SidebarViewModel.Projects
                    .Where(p => p.IsFavorite)
                    .Select(p => (object)p)
                    .Concat(SidebarViewModel.WorkSpaces.Where(w => w.IsFavorite).Select(w => (object)w))
                    .OrderByDescending(item => item is ProjectViewModel p ? p.FavoritedAt : ((WorkSpaceViewModel)item).FavoritedAt)
                    .ToList();
                foreach (var item in favoriteItems)
                {
                    ContentItems.Add(item);
                }
                break;

            case TreeItemType.WorkSpace:
                // 显示所有工作空间
                foreach (var workSpace in SidebarViewModel.WorkSpaces)
                {
                    ContentItems.Add(workSpace);
                }
                break;

            case TreeItemType.WorkFolder:
                // 从数据库加载该文件夹下的项目和工作空间
                var folderProjects = await _projectAppService.GetByWorkFolderIdAsync(selectedItem.Id);
                var folderWorkSpaces = await _workSpaceAppService.GetByWorkFolderIdAsync(selectedItem.Id);

                // 添加项目
                foreach (var project in folderProjects)
                {
                    var projectVm = new ProjectViewModel(project, _dialogService, _serviceProvider, _projectAppService);
                    ContentItems.Add(projectVm);
                }

                // 添加工作空间
                foreach (var workSpace in folderWorkSpaces)
                {
                    var workSpaceVm = new WorkSpaceViewModel(workSpace, _dialogService, _serviceProvider, _workSpaceAppService);
                    ContentItems.Add(workSpaceVm);
                }
                break;

            case TreeItemType.TagSettings:
                // 标签设置页面，可以显示所有标签或空
                break;
        }

        Logger.LogDebug($"内容区域已更新: {selectedItem.ItemType}, 共 {ContentItems.Count} 项");
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 处理侧边栏选中项变化
    /// </summary>
    private void OnSidebarSelectedItemChanged(object? sender, TreeItemViewModel? selectedItem)
    {
        if (selectedItem != null)
        {
            UpdateContentItems(selectedItem);
        }
    }

    /// <summary>
    /// 添加内容
    /// </summary>
    private async Task AddContentAsync()
    {
        try
        {
            if (SidebarViewModel.SelectedTreeItem == null) return;

            switch (SidebarViewModel.SelectedTreeItem.ItemType)
            {
                case TreeItemType.AllProjects:
                    await AddProjectAsync();
                    break;

                case TreeItemType.WorkSpace:
                    await AddWorkSpaceAsync();
                    break;

                case TreeItemType.WorkFolder:
                    // 在工作文件夹下添加项目
                    await AddProjectToFolderAsync(SidebarViewModel.SelectedTreeItem.Id);
                    break;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(
                L.Message_SaveFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    private async Task AddProjectAsync()
    {
        // 显示添加项目对话框
        var viewModel = _serviceProvider.GetRequiredService<ProjectDialogViewModel>();
        viewModel.InitializeForAdd();
        var result = await _dialogService.ShowDialogAsync<ProjectDialogViewModel, ProjectDto?>(viewModel);

        if (result.Confirmed && result.Value != null)
        {
            Logger.LogInformation("项目创建成功: {ProjectName}", result.Value.Name);

            // 直接添加新项目到集合
            var projectVm = new ProjectViewModel(result.Value, _dialogService, _serviceProvider, _projectAppService);
            SidebarViewModel.Projects.Add(projectVm);

            // 更新侧边栏计数
            SidebarViewModel.IncrementProjectCount();

            // 刷新内容区域显示
            if (SidebarViewModel.SelectedTreeItem != null &&
                (SidebarViewModel.SelectedTreeItem.ItemType == TreeItemType.AllProjects ||
                 SidebarViewModel.SelectedTreeItem.ItemType == TreeItemType.RecentProject ||
                 SidebarViewModel.SelectedTreeItem.ItemType == TreeItemType.FavoriteProject))
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
            }

            // 更新统计
            SidebarViewModel.UpdateStatistics();

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_ProjectCreated, result.Value.Name),
                NotificationType.Success,
                3000);
        }
        else
        {
            Logger.LogInformation("用户取消创建项目");
        }
    }

    /// <summary>
    /// 添加工作空间
    /// </summary>
    private async Task AddWorkSpaceAsync()
    {
        // 显示添加工作空间对话框
        var viewModel = _serviceProvider.GetRequiredService<WorkSpaceDialogViewModel>();
        await viewModel.InitializeForAddAsync();
        var result = await _dialogService.ShowDialogAsync<WorkSpaceDialogViewModel, WorkSpaceDto?>(viewModel);

        if (result.Confirmed && result.Value != null)
        {
            Logger.LogInformation("工作空间创建成功: {WorkSpaceName}", result.Value.Name);

            // 直接添加新工作空间到集合
            var workSpaceVm = new WorkSpaceViewModel(result.Value, _dialogService, _serviceProvider, _workSpaceAppService);
            SidebarViewModel.WorkSpaces.Add(workSpaceVm);

            // 更新侧边栏工作空间节点计数
            SidebarViewModel.IncrementWorkSpaceCount();

            // 刷新内容区域显示
            if (SidebarViewModel.SelectedTreeItem?.ItemType == TreeItemType.WorkSpace)
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
            }

            // 更新统计
            SidebarViewModel.UpdateStatistics();

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_WorkSpaceCreated, result.Value.Name),
                NotificationType.Success,
                3000);
        }
        else
        {
            Logger.LogInformation("用户取消创建工作空间");
        }
    }

    /// <summary>
    /// 在指定文件夹下添加项目
    /// </summary>
    private async Task AddProjectToFolderAsync(long? folderId)
    {
        // 显示添加项目对话框
        var viewModel = _serviceProvider.GetRequiredService<ProjectDialogViewModel>();
        viewModel.InitializeForAdd();
        var result = await _dialogService.ShowDialogAsync<ProjectDialogViewModel, ProjectDto?>(viewModel);

        if (result.Confirmed && result.Value != null)
        {
            // 如果指定了文件夹，将项目移动到该文件夹
            if (folderId.HasValue)
            {
                await _workFolderAppService.MoveProjectToWorkFolderAsync(result.Value.Id, folderId.Value);
            }

            Logger.LogInformation("项目创建成功: {ProjectName}", result.Value.Name);

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_ProjectCreated, result.Value.Name),
                NotificationType.Success,
                3000);
        }
        else
        {
            Logger.LogInformation("用户取消创建项目");
        }
    }

    #endregion
}
