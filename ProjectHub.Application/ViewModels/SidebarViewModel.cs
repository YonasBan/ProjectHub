using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.ViewModels.DialogViewModel;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 侧边栏 ViewModel - 管理侧边栏树形结构、选中状态和统计
/// </summary>
public class SidebarViewModel : ViewModelBase
{
    #region 注入服务

    private readonly IProjectAppService _projectAppService;
    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly IDialogService _dialogService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileExplorerService _fileExplorerService;

    #endregion

    #region 数据集合属性

    /// <summary>
    /// 侧边栏树形节点集合
    /// </summary>
    private ObservableCollection<TreeItemViewModel>? _sidebarTreeItems;

    public ObservableCollection<TreeItemViewModel> SidebarTreeItems
    {
        get => _sidebarTreeItems ??= new();
        set => this.RaiseAndSetIfChanged(ref _sidebarTreeItems, value);
    }

    #endregion

    #region 选中状态属性

    /// <summary>
    /// 当前选中的树形节点
    /// </summary>
    private TreeItemViewModel _selectedTreeItem;

    public TreeItemViewModel SelectedTreeItem
    {
        get => _selectedTreeItem;
        set => this.RaiseAndSetIfChanged(ref _selectedTreeItem, value);
    }

    #endregion

    #region 数据集合属性

    /// <summary>
    /// 项目列表 (Observable)
    /// </summary>
    private ObservableCollection<ProjectViewModel>? _projects;

    public ObservableCollection<ProjectViewModel> Projects
    {
        get => _projects ??= new();
        set => this.RaiseAndSetIfChanged(ref _projects, value);
    }

    /// <summary>
    /// 工作文件夹列表 (Observable)
    /// </summary>
    private ObservableCollection<WorkFolderViewModel>? _workFolders;

    public ObservableCollection<WorkFolderViewModel> WorkFolders
    {
        get => _workFolders ??= new();
        set => this.RaiseAndSetIfChanged(ref _workFolders, value);
    }

    /// <summary>
    /// 工作空间列表 (Observable)
    /// </summary>
    private ObservableCollection<WorkSpaceViewModel>? _workSpaces;

    public ObservableCollection<WorkSpaceViewModel> WorkSpaces
    {
        get => _workSpaces ??= new();
        set => this.RaiseAndSetIfChanged(ref _workSpaces, value);
    }

    #endregion

    #region 状态栏统计属性

    /// <summary>
    /// 所有项目总数
    /// </summary>
    private int _totalProjectCount;

    public int TotalProjectCount
    {
        get => _totalProjectCount;
        set => this.RaiseAndSetIfChanged(ref _totalProjectCount, value);
    }

    /// <summary>
    /// 收藏项目数量
    /// </summary>
    private int _favoriteProjectCount;

    public int FavoriteCount
    {
        get => _favoriteProjectCount;
        set => this.RaiseAndSetIfChanged(ref _favoriteProjectCount, value);
    }

    /// <summary>
    /// 工作文件夹总数
    /// </summary>
    private int _totalFolderCount;

    public int TotalFolderCount
    {
        get => _totalFolderCount;
        set => this.RaiseAndSetIfChanged(ref _totalFolderCount, value);
    }

    /// <summary>
    /// 工作空间总数
    /// </summary>
    private int _totalWorkspaceCount;

    public int TotalWorkspaceCount
    {
        get => _totalWorkspaceCount;
        set => this.RaiseAndSetIfChanged(ref _totalWorkspaceCount, value);
    }

    #endregion

    #region 状态栏格式化文本

    /// <summary>
    /// 项目计数显示文本
    /// </summary>
    private string _projectCountText = string.Empty;

    public string ProjectCountText
    {
        get => _projectCountText;
        set => this.RaiseAndSetIfChanged(ref _projectCountText, value);
    }

    /// <summary>
    /// 文件夹计数显示文本
    /// </summary>
    private string _folderCountText = string.Empty;

    public string FolderCountText
    {
        get => _folderCountText;
        set => this.RaiseAndSetIfChanged(ref _folderCountText, value);
    }

    /// <summary>
    /// 工作空间计数显示文本
    /// </summary>
    private string _workspaceCountText = string.Empty;

    public string WorkspaceCountText
    {
        get => _workspaceCountText;
        set => this.RaiseAndSetIfChanged(ref _workspaceCountText, value);
    }


    /// <summary>
    /// 标签项目显示文本
    /// </summary>
    private string _favouriteText = string.Empty;

    public string FavouriteText
    {
        get => _favouriteText;
        set => this.RaiseAndSetIfChanged(ref _favouriteText, value);
    }

    #endregion

    #region Reactive Commands

    /// <summary>
    /// 创建文件夹命令
    /// </summary>
    public ReactiveCommand<TreeItemViewModel, Unit> CreateFolderCommand { get; }

    /// <summary>
    /// 删除文件夹命令
    /// </summary>
    public ReactiveCommand<TreeItemViewModel, Unit> DeleteFolderCommand { get; }

    #endregion

    #region 事件

    /// <summary>
    /// 选中项改变事件
    /// </summary>
    public event EventHandler<TreeItemViewModel?>? SelectedItemChanged;

    #endregion

    #region 构造函数

    public SidebarViewModel(
        ILogger<SidebarViewModel> logger,
        IProjectAppService projectAppService,
        IWorkFolderAppService workFolderAppService,
        IWorkSpaceAppService workSpaceAppService,
        IScheduler mainThreadScheduler,
        IDialogService dialogService,
        IServiceProvider serviceProvider,
        IFileExplorerService fileExplorerService)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workFolderAppService = workFolderAppService;
        _workSpaceAppService = workSpaceAppService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
        _fileExplorerService = fileExplorerService;

        CreateFolderCommand = ReactiveCommand.CreateFromTask<TreeItemViewModel>(CreateFolderAsync);
        DeleteFolderCommand = ReactiveCommand.CreateFromTask<TreeItemViewModel>(DeleteFolderAsync);

        // 订阅命令异常
        CreateFolderCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "创建文件夹时发生错误"));
        DeleteFolderCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "删除文件夹时发生错误"));

        // 订阅语言切换事件
        L.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                RefreshSidebarTreeNames();
            })
            .DisposeWith(Disposables);

        // 订阅选中项变化
        this.WhenAnyValue(x => x.SelectedTreeItem)
            .Subscribe(item => SelectedItemChanged?.Invoke(this, item));

        // 订阅 MessageBus 消息
        SubscribeToMessageBus();
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 构建侧边栏树形结构
    /// </summary>
    public void BuildSidebarTree()
    {
        if (SidebarTreeItems == null)
            return;

        SidebarTreeItems.Clear();
        var recent = new TreeItemViewModel(L.Sidebar_Recent, 0, TreeItemType.RecentProject)
        { IsSelected = true };
        SidebarTreeItems.Add(recent);

        var favorites = new TreeItemViewModel(L.Sidebar_Favorites, 0, TreeItemType.FavoriteProject);
        SidebarTreeItems.Add(favorites);

        // Workspaces (工作空间)
        var workspaces = new TreeItemViewModel(L.Sidebar_Workspaces, WorkSpaces.Count, TreeItemType.WorkSpace);
        SidebarTreeItems.Add(workspaces);

        // Work Folders (工作文件夹) - 支持树形结构
        var folderNodes = WorkFolders
            .OrderBy(f => f.SortOrder)
            .ToDictionary(f => f.Id, f => new TreeItemViewModel(f.Name, 0, TreeItemType.WorkFolder, f.Id));
        foreach (var folder in WorkFolders.OrderBy(f => f.SortOrder))
        {
            if (folder.ParentId.HasValue && folderNodes.TryGetValue(folder.ParentId.Value, out var parentNode))
            {
                parentNode.Children.Add(folderNodes[folder.Id]);
            }
            else
            {
                SidebarTreeItems.Add(folderNodes[folder.Id]);
            }
        }

        // All Projects (所有项目)
        var allProjects = new TreeItemViewModel(L.Sidebar_AllProjects, Projects.Count, TreeItemType.AllProjects);
        SidebarTreeItems.Add(allProjects);

        //// Tag Settings (标签设置)
        //var tagSettings = new TreeItemViewModel(L.Sidebar_TagSettings, 0, TreeItemType.TagSettings);
        //SidebarTreeItems.Add(tagSettings);

        Logger.LogDebug("侧边栏树形结构已重建");
    }

    /// <summary>
    /// 重建文件夹树形结构（保持其他节点不变）
    /// </summary>
    public void RebuildFolderTreeOnly()
    {
        // 移除所有文件夹节点
        var folderNodesToRemove = SidebarTreeItems.Where(i => i.ItemType == TreeItemType.WorkFolder).ToList();
        foreach (var node in folderNodesToRemove)
        {
            SidebarTreeItems.Remove(node);
        }

        // 找到工作空间节点的索引
        var workSpaceIndex = -1;
        for (int i = 0; i < SidebarTreeItems.Count; i++)
        {
            if (SidebarTreeItems[i].ItemType == TreeItemType.WorkSpace)
            {
                workSpaceIndex = i;
                break;
            }
        }

        // 重新添加文件夹节点
        var insertIndex = workSpaceIndex + 1;
        var folderNodes = WorkFolders
            .OrderBy(f => f.SortOrder)
            .ToDictionary(f => f.Id, f => new TreeItemViewModel(f.Name, 0, TreeItemType.WorkFolder, f.Id));

        foreach (var folder in WorkFolders.OrderBy(f => f.SortOrder))
        {
            if (folder.ParentId.HasValue && folderNodes.TryGetValue(folder.ParentId.Value, out var parentNode))
            {
                parentNode.Children.Add(folderNodes[folder.Id]);
            }
            else
            {
                SidebarTreeItems.Insert(insertIndex, folderNodes[folder.Id]);
                insertIndex++;
            }
        }
    }

    /// <summary>
    /// 增加项目计数
    /// </summary>
    public void IncrementProjectCount()
    {
        var projectTreeView = SidebarTreeItems.FirstOrDefault(r => r.ItemType == TreeItemType.AllProjects);
        if (projectTreeView != null)
            projectTreeView.Count++;
    }

    /// <summary>
    /// 增加工作空间计数
    /// </summary>
    public void IncrementWorkSpaceCount()
    {
        var workSpaceTreeView = SidebarTreeItems.FirstOrDefault(r => r.ItemType == TreeItemType.WorkSpace);
        if (workSpaceTreeView != null)
            workSpaceTreeView.Count++;
    }


    /// <summary>
    /// 更新统计信息
    /// </summary>
    public void UpdateStatistics()
    {
        TotalProjectCount = Projects.Count;
        FavoriteCount = Projects.Count(p => p.IsFavorite) + WorkSpaces.Count(w => w.IsFavorite);
        TotalFolderCount = WorkFolders.Count;
        TotalWorkspaceCount = WorkSpaces.Count;

        // 更新格式化后的状态栏文本（支持语言切换）
        ProjectCountText = string.Format(L.Status_ProjectCount, TotalProjectCount);
        FolderCountText = string.Format(L.Status_FolderCount, TotalFolderCount);
        WorkspaceCountText = string.Format(L.Status_WorkspaceCount, TotalWorkspaceCount);
        FavouriteText = string.Format(L.Status_FavoriteCount, FavoriteCount);

        // 更新 SidebarTreeItems 中的计数
        UpdateTreeItemCounts();
    }

    /// <summary>
    /// 更新 SidebarTreeItems 中各节点的计数
    /// </summary>
    private void UpdateTreeItemCounts()
    {
        if (SidebarTreeItems == null || SidebarTreeItems.Count == 0)
            return;
        foreach (var item in SidebarTreeItems)
        {
            switch (item.ItemType)
            {
                case TreeItemType.RecentProject:
                    // 最近使用 - 包含项目和工作空间（最多显示20个）
                    var recentProjectCount = Projects.Count(p => p.LastOpenedAt.HasValue);
                    var recentWorkSpaceCount = WorkSpaces.Count(w => w.LastOpenedAt.HasValue);
                    item.Count = Math.Min(recentProjectCount + recentWorkSpaceCount, 20);
                    break;

                case TreeItemType.FavoriteProject:
                    // 收藏夹 - 包含项目和工作空间
                    var favoriteProjectCount = Projects.Count(p => p.IsFavorite);
                    var favoriteWorkSpaceCount = WorkSpaces.Count(w => w.IsFavorite);
                    item.Count = favoriteProjectCount + favoriteWorkSpaceCount;
                    break;

                case TreeItemType.WorkSpace:
                    // 工作空间
                    item.Count = WorkSpaces.Count;
                    break;

                case TreeItemType.AllProjects:
                    // 所有项目
                    item.Count = Projects.Count;
                    break;
            }
        }
    }

    #endregion

    #region 数据加载方法

    /// <summary>
    /// 加载所有项目
    /// </summary>
    public async Task LoadProjectsAsync()
    {
        try
        {
            Logger.LogInformation("开始加载项目列表");

            var projects = await _projectAppService.GetAllActiveAsync();

            Projects.Clear();
            foreach (var project in projects)
            {
                var projectVm = new ProjectViewModel(project, _dialogService, _serviceProvider, _projectAppService, _fileExplorerService);
                Projects.Add(projectVm);
            }

            Logger.LogInformation($"成功加载 {projects.Count} 个项目");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载项目列表时发生错误");
        }
    }

    /// <summary>
    /// 加载工作文件夹
    /// </summary>
    public async Task LoadWorkFoldersAsync()
    {
        try
        {
            Logger.LogInformation("加载工作文件夹列表");

            var workFolders = await _workFolderAppService.GetAllWithProjectCountAsync();

            WorkFolders.Clear();
            foreach (var workFolder in workFolders)
            {
                WorkFolders.Add(new WorkFolderViewModel(workFolder));
            }

            Logger.LogInformation($"成功加载 {workFolders.Count} 个工作文件夹");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载工作文件夹列表时发生错误");
        }
    }

    /// <summary>
    /// 加载工作空间
    /// </summary>
    public async Task LoadWorkSpacesAsync()
    {
        try
        {
            Logger.LogInformation("加载工作空间列表");

            var workSpaces = await _workSpaceAppService.GetAllWithProjectCountAsync();

            WorkSpaces.Clear();
            foreach (var workSpace in workSpaces)
            {
                var workSpaceVm = new WorkSpaceViewModel(workSpace, _dialogService, _serviceProvider, _workSpaceAppService);
                WorkSpaces.Add(workSpaceVm);
            }

            Logger.LogInformation($"成功加载 {workSpaces.Count} 个工作空间");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载工作空间列表时发生错误");
        }
    }

    /// <summary>
    /// 首次加载数据
    /// </summary>
    public async Task LoadInitialDataAsync()
    {
        // 加载项目数据
        await LoadProjectsAsync();

        // 加载工作文件夹
        await LoadWorkFoldersAsync();

        // 加载工作空间数据
        await LoadWorkSpacesAsync();

        // 构建侧边栏树形结构
        BuildSidebarTree();
        // 更新统计
        UpdateStatistics();
        // 默认选中"最近使用"
        if (SidebarTreeItems.Count > 0)
        {
            SelectedTreeItem = SidebarTreeItems.First();
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 刷新侧边栏树节点名称
    /// </summary>
    private void RefreshSidebarTreeNames()
    {
        foreach (var item in SidebarTreeItems)
        {
            switch (item.ItemType)
            {
                case TreeItemType.RecentProject:
                    item.UpdateName(L.Sidebar_Recent);
                    break;
                case TreeItemType.FavoriteProject:
                    item.UpdateName(L.Sidebar_Favorites);
                    break;
                case TreeItemType.WorkSpace:
                    item.UpdateName(L.Sidebar_Workspaces);
                    break;
                case TreeItemType.AllProjects:
                    item.UpdateName(L.Sidebar_AllProjects);
                    break;
                case TreeItemType.TagSettings:
                    item.UpdateName(L.Sidebar_TagSettings);
                    break;
            }

            if (SelectedTreeItem?.ItemType == item.ItemType && item.ItemType != TreeItemType.WorkFolder)
            {
                SelectedTreeItem.Name = item.Name;
            }
        }
    }

    /// <summary>
    /// 创建文件夹
    /// </summary>
    private async Task CreateFolderAsync(TreeItemViewModel parent)
    {
        try
        {
            Logger.LogInformation("开始创建文件夹");
            // 显示对话框
            var result = await _dialogService.ShowDialogAsync<CreateFolderDialogViewModel, string>();

            if (result.Confirmed && !string.IsNullOrWhiteSpace(result.Value))
            {
                var folderName = (string)result.Value!;

                // 创建文件夹 DTO
                var createDto = new CreateWorkFolderDto
                {
                    Name = folderName,
                    ParentId = parent?.Id // 如果选中了父文件夹，则创建为子文件夹
                };

                // 调用应用服务创建
                var newFolder = await _workFolderAppService.CreateAsync(createDto);

                Logger.LogInformation("成功创建文件夹: {FolderName}, ID: {FolderId}", folderName, newFolder.Id);
                if (parent != null)
                {
                    parent.Children.Add(new TreeItemViewModel(newFolder.Name, 0, TreeItemType.WorkFolder, newFolder.Id));
                }
                else
                {
                    var index = -1;
                    for (int i = SidebarTreeItems.Count - 1; i >= 0; i--)
                    {
                        if (SidebarTreeItems[i].ItemType == TreeItemType.WorkFolder)
                        {
                            index = i;
                            break;
                        }
                        else if (SidebarTreeItems[i].ItemType == TreeItemType.WorkSpace)
                        {
                            index = i;
                            break;
                        }
                    }
                    this.SidebarTreeItems.Insert(index + 1, new TreeItemViewModel(newFolder.Name, 0, TreeItemType.WorkFolder, newFolder.Id));
                }

            }
            else
            {
                Logger.LogInformation("用户取消创建文件夹");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "创建文件夹时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_SaveFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    private async Task DeleteFolderAsync(TreeItemViewModel current)
    {
        if (current == null) return;

        try
        {
            Logger.LogInformation($"请求删除文件夹: {current.Name}");

            // 显示确认对话框
            var confirmed = await _dialogService.ShowConfirmAsync(
                L.DeleteConfirm_Title,
                string.Format(L.DeleteConfirm_Message, current.Name));

            if (!confirmed)
            {
                Logger.LogInformation("用户取消删除文件夹");
                return;
            }


            // 执行删除
            await _workFolderAppService.DeleteAsync(current.Id);

            Logger.LogInformation($"文件夹删除成功: {current.Name}");

            // 从侧边栏树中移除（查找父节点）
            var parentNode = FindParentNode(SidebarTreeItems, current);
            if (parentNode != null)
            {
                // 从父节点的 Children 中移除
                parentNode.Children.Remove(current);
            }
            else
            {
                // 从根节点移除
                SidebarTreeItems.Remove(current);
            }

            _dialogService.ShowNotification(
                string.Format(L.Message_FolderDeleted, current.Name),
                NotificationType.Success,
                3000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除文件夹时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_DeleteFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// 在树中查找指定节点的父节点
    /// </summary>
    private TreeItemViewModel? FindParentNode(ObservableCollection<TreeItemViewModel> nodes, TreeItemViewModel target)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Contains(target))
            {
                return node;
            }

            // 递归查找子节点
            var foundInChildren = FindParentNode(node.Children, target);
            if (foundInChildren != null)
            {
                return foundInChildren;
            }
        }
        return null;
    }

    #endregion

    #region MessageBus 消息处理

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


        // 移动到文件夹请求消息
        MessageBus.Current.Listen<ProjectMoveToFolderRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectMoveToFolderRequested(msg.Project))
            .DisposeWith(Disposables);

        MessageBus.Current.Listen<WorkSpaceMoveToFolderRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnWorkSpaceMoveToFolderRequested(msg.WorkSpace))
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// 处理工作空间已删除（刷新列表）
    /// </summary>
    private void OnWorkSpaceDeleted(WorkSpaceViewModel workSpaceVm)
    {
        // 从列表中移除
        WorkSpaces.Remove(workSpaceVm);
        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 处理工作空间收藏状态变化
    /// </summary>
    private void OnWorkSpaceFavoriteChanged(WorkSpaceViewModel workSpaceVm)
    {
        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 处理项目已删除（刷新列表）
    /// </summary>
    private void OnProjectDeleted(ProjectViewModel projectVm)
    {
        // 从列表中移除
        Projects.Remove(projectVm);
        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 处理项目收藏状态变化
    /// </summary>
    private void OnProjectFavoriteChanged(ProjectViewModel projectVm)
    {
        // 更新统计
        UpdateStatistics();
    }
    /// <summary>
    /// 处理项目移动到文件夹请求
    /// </summary>
    private async void OnProjectMoveToFolderRequested(ProjectViewModel projectVm)
    {
        await ShowFolderSelectorAndMoveAsync(projectVm, isProject: true);
    }

    /// <summary>
    /// 处理工作空间移动到文件夹请求
    /// </summary>
    private async void OnWorkSpaceMoveToFolderRequested(WorkSpaceViewModel workSpaceVm)
    {
        await ShowFolderSelectorAndMoveAsync(workSpaceVm, isProject: false);
    }

    /// <summary>
    /// 显示文件夹选择器并执行移动操作（使用已加载的SidebarTreeItems文件夹树）
    /// </summary>
    private async Task ShowFolderSelectorAndMoveAsync(object itemVm, bool isProject)
    {
        try
        {
            var itemName = isProject ? ((ProjectViewModel)itemVm).Name : ((WorkSpaceViewModel)itemVm).Name;
            var itemId = isProject ? ((ProjectViewModel)itemVm).Id : ((WorkSpaceViewModel)itemVm).Id;

            // 创建对话框 ViewModel（直接注入 IWorkFolderAppService）
            var dialogVm = new SelectFolderDialogViewModel(_workFolderAppService)
            {
                Title = string.Format(L.Dialog_MoveToFolderTitle, itemName),
                ItemName = itemName,
                MoveItem = new MoveItemInfo
                {
                    Id = itemId,
                    Name = itemName,
                    IsProject = isProject
                }
            };

            // 直接从 SidebarTreeItems 加载文件夹树（传递引用，新建文件夹时会自动更新）
            dialogVm.LoadFoldersFromSidebar(SidebarTreeItems);

            // 显示对话框
            var result = await _dialogService.ShowDialogAsync(dialogVm);

            if (!result)
            {
                Logger.LogInformation("用户取消移动到文件夹");
                return;
            }

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_MovedToFolder, itemName),
                NotificationType.Success,
                3000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "移动到文件夹时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_MoveToFolderFailed,
                ex.Message);
        }
    }

    #endregion
}
