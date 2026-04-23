using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels.DialogViewModel;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

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
    private readonly IFileExplorerService _fileExplorerService;

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

    #region 状态属性

    /// <summary>
    /// 当前选中的树节点（用于搜索后恢复）
    /// </summary>
    private TreeItemViewModel? _currentSelectedItem;

    /// <summary>
    /// 当前是否选中了工作文件夹节点
    /// </summary>
    private bool _isWorkFolderSelected;
    public bool IsWorkFolderSelected
    {
        get => _isWorkFolderSelected;
        private set => this.RaiseAndSetIfChanged(ref _isWorkFolderSelected, value);
    }

    /// <summary>
    /// 当前选中的工作文件夹ID（用于移除操作）
    /// </summary>
    private long? _currentWorkFolderId;
    public long? CurrentWorkFolderId
    {
        get => _currentWorkFolderId;
        private set => this.RaiseAndSetIfChanged(ref _currentWorkFolderId, value);
    }

    /// <summary>
    /// 当前是否处于搜索模式
    /// </summary>
    private bool _isSearching;
    public bool IsSearching
    {
        get => _isSearching;
        private set => this.RaiseAndSetIfChanged(ref _isSearching, value);
    }

    /// <summary>
    /// 搜索关键字
    /// </summary>
    private string _searchKeyword = string.Empty;
    public string SearchKeyword
    {
        get => _searchKeyword;
        set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
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

    /// <summary>
    /// 从当前文件夹移除项目或工作空间命令
    /// </summary>
    public ReactiveCommand<object, Unit> RemoveFromFolderCommand { get; }

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
        SidebarViewModel sidebarViewModel,
        IFileExplorerService fileExplorerService)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workSpaceAppService = workSpaceAppService;
        _workFolderAppService = workFolderAppService;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        _fileExplorerService = fileExplorerService;
        SidebarViewModel = sidebarViewModel;

        AddContentCommand = ReactiveCommand.CreateFromTask(AddContentAsync);
        RemoveFromFolderCommand = ReactiveCommand.CreateFromTask<object>(RemoveFromFolderAsync);

        // 订阅侧边栏选中项变化，自动更新内容
        SidebarViewModel.SelectedItemChanged += OnSidebarSelectedItemChanged;
        SubscribeToMessageBus();
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 根据选中的树节点更新内容项
    /// </summary>
    public async void UpdateContentItems(TreeItemViewModel selectedItem)
    {
        _currentSelectedItem = selectedItem;
        ContentItems.Clear();
        IsWorkFolderSelected = false;
        IsSearching = false;
        SearchKeyword = string.Empty;
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
                // 设置当前文件夹状态
                IsWorkFolderSelected = true;
                CurrentWorkFolderId = selectedItem.Id;

                // 从数据库加载该文件夹下的项目和工作空间ID
                var folderProjectIds = (await _projectAppService.GetByWorkFolderIdAsync(selectedItem.Id))
                    .Select(p => p.Id)
                    .ToHashSet();
                var folderWorkSpaceIds = (await _workSpaceAppService.GetByWorkFolderIdAsync(selectedItem.Id))
                    .Select(w => w.Id)
                    .ToHashSet();

                // 从现有 SidebarViewModel 集合中筛选并添加（避免重复创建 ViewModel）
                foreach (var project in SidebarViewModel.Projects.Where(p => folderProjectIds.Contains(p.Id)))
                {
                    ContentItems.Add(project);
                }

                foreach (var workSpace in SidebarViewModel.WorkSpaces.Where(w => folderWorkSpaceIds.Contains(w.Id)))
                {
                    ContentItems.Add(workSpace);
                }
                break;

            default:
                // 其他情况重置文件夹状态
                IsWorkFolderSelected = false;
                CurrentWorkFolderId = null;
                break;
        }

        Logger.LogDebug($"内容区域已更新: {selectedItem.ItemType}, 共 {ContentItems.Count} 项");
    }

    /// <summary>
    /// 执行搜索，从项目和工作空间中过滤匹配项
    /// </summary>
    public void Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            // 清空搜索，恢复原来的导航状态
            IsSearching = false;
            SearchKeyword = string.Empty;
            if (_currentSelectedItem != null)
            {
                UpdateContentItems(_currentSelectedItem);
            }
            return;
        }

        IsSearching = true;
        SearchKeyword = keyword;
        ContentItems.Clear();
        IsWorkFolderSelected = false;
        CurrentWorkFolderId = null;

        var lowerKeyword = keyword.ToLowerInvariant();

        // 从项目中过滤
        var matchedProjects = SidebarViewModel.Projects
            .Where(p => MatchesSearch(p, lowerKeyword))
            .Select(p => (object)p);

        // 从工作空间中过滤
        var matchedWorkSpaces = SidebarViewModel.WorkSpaces
            .Where(w => MatchesSearch(w, lowerKeyword))
            .Select(w => (object)w);

        // 合并结果
        var results = matchedProjects.Concat(matchedWorkSpaces).ToList();
        foreach (var item in results)
        {
            ContentItems.Add(item);
        }

        Logger.LogDebug($"搜索完成: 关键字 '{keyword}', 共 {ContentItems.Count} 项匹配");
    }

    /// <summary>
    /// 判断 ViewModel 是否匹配搜索关键字
    /// </summary>
    private static bool MatchesSearch(object item, string lowerKeyword)
    {
        return item switch
        {
            ProjectViewModel p =>
                p.Name.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase) ||
                p.Path.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase)),
            WorkSpaceViewModel w =>
                w.Name.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(w.Description) && w.Description.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
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
            var projectVm = new ProjectViewModel(result.Value, _dialogService, _serviceProvider, _projectAppService, _fileExplorerService);
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

    /// <summary>
    /// 从当前文件夹移除项目或工作空间
    /// </summary>
    private async Task RemoveFromFolderAsync(object item)
    {
        if (!CurrentWorkFolderId.HasValue) return;

        try
        {
            switch (item)
            {
                case ProjectViewModel project:
                    await _workFolderAppService.RemoveProjectFromFolderAsync(project.Id, CurrentWorkFolderId.Value);
                    ContentItems.Remove(item);
                    _dialogService.ShowNotification(
                        string.Format(L.Message_RemovedFromFolder, project.Name),
                        NotificationType.Success,
                        3000);
                    break;

                case WorkSpaceViewModel workSpace:
                    await _workFolderAppService.RemoveWorkSpaceFromFolderAsync(workSpace.Id, CurrentWorkFolderId.Value);
                    ContentItems.Remove(item);
                    _dialogService.ShowNotification(
                        string.Format(L.Message_RemovedFromFolder, workSpace.Name),
                        NotificationType.Success,
                        3000);
                    break;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(L.Message_SaveFailed, ex.Message);
        }
    }


    /// <summary>
    /// 订阅 MessageBus 消息
    /// </summary>
    private void SubscribeToMessageBus()
    {
        // 项目已删除消息（用于刷新列表）
        MessageBus.Current.Listen<ProjectDeletedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectDeleted(msg.Project))
            .DisposeWith(Disposables);

        // 项目收藏状态变化
        MessageBus.Current.Listen<ProjectFavoriteChangedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectFavoriteChanged(msg.Project))
            .DisposeWith(Disposables);

        // 工作空间已删除消息（用于刷新列表）
        MessageBus.Current.Listen<WorkSpaceDeletedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnWorkSpaceDeleted(msg.WorkSpace))
            .DisposeWith(Disposables);

        // 工作空间收藏状态变化
        MessageBus.Current.Listen<WorkSpaceFavoriteChangedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnWorkSpaceFavoriteChanged(msg.WorkSpace))
            .DisposeWith(Disposables);
    }

    private void OnWorkSpaceFavoriteChanged(WorkSpaceViewModel workSpace)
    {
        if (SidebarViewModel.SelectedTreeItem.ItemType == TreeItemType.FavoriteProject)
        {
            if (workSpace.IsFavorite)
            {
                this.ContentItems.Add(workSpace);
            }
            else
            {
                this.ContentItems.Remove(workSpace);
            }
        }
    }

    private void OnWorkSpaceDeleted(WorkSpaceViewModel workSpace)
    {
        if (this.ContentItems.Contains(workSpace))
        {
            this.ContentItems.Remove(workSpace);
        }
    }

    private void OnProjectFavoriteChanged(ProjectViewModel project)
    {
        if (SidebarViewModel.SelectedTreeItem.ItemType == TreeItemType.FavoriteProject)
        {
            if (project.IsFavorite)
            {
                this.ContentItems.Add(project);
            }
            else
            {
                this.ContentItems.Remove(project);
            }
        }
    }

    private void OnProjectDeleted(ProjectViewModel project)
    {
        if (this.ContentItems.Contains(project))
        {
            this.ContentItems.Remove(project);
        }
    }

    #endregion
}
