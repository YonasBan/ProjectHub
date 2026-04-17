using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 主窗口 ViewModel
///
/// ⚠️ 注意：此文件仅为框架示例，实际实现需要在第二阶段完成
/// </summary>
public class MainViewModel : ViewModelBase
{
    #region 注入服务

    private readonly IProjectAppService _projectAppService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly IDialogService _dialogService;
    private readonly IThemeService _themeService;

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

    #region 选中状态属性

    /// <summary>
    /// 当前选中的工作文件夹
    /// </summary>
    private WorkFolderViewModel? _selectedWorkFolder;

    public WorkFolderViewModel? SelectedWorkFolder
    {
        get => _selectedWorkFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkFolder, value);
    }

    /// <summary>
    /// 当前选中的树形节点
    /// </summary>
    private TreeItemViewModel? _selectedTreeItem;

    public TreeItemViewModel? SelectedTreeItem
    {
        get => _selectedTreeItem;
        set => this.RaiseAndSetIfChanged(ref _selectedTreeItem, value);
    }

    #endregion

    #region 搜索与视图属性

    /// <summary>
    /// 搜索关键字
    /// </summary>
    private string _searchKeyword = string.Empty;

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
    }

    /// <summary>
    /// 当前视图模式 (列表/卡片)
    /// </summary>
    private ViewMode _currentViewMode = ViewMode.List;

    public ViewMode CurrentViewMode
    {
        get => _currentViewMode;
        set => this.RaiseAndSetIfChanged(ref _currentViewMode, value);
    }

    #endregion

    #region Reactive Commands


    /// <summary>
    /// 刷新数据命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// 创建文件夹命令
    /// </summary>
    public ReactiveCommand<TreeItemViewModel, Unit> CreateFolderCommand { get; }

    /// <summary>
    /// 删除文件夹命令
    /// </summary>
    public ReactiveCommand<TreeItemViewModel, Unit> DeleteFolderCommand { get; }

    /// <summary>
    /// 添加内容命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddContentCommand { get; }

    /// <summary>
    /// 切换语言命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleLanguageCommand { get; }

    /// <summary>
    /// 切换主题命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }

    #endregion

    #region 主题与状态属性

    /// <summary>
    /// 当前主题 (Light/Dark)
    /// </summary>
    public string CurrentTheme => _themeService.CurrentTheme;

    /// <summary>
    /// 正在加载标识
    /// </summary>
    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    #endregion

    #region 侧边栏统计属性

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

    public int FavoriteProjectCount
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

    /// <summary>
    /// 标签分类数量
    /// </summary>
    private int _tagCount;

    public int TagCount
    {
        get => _tagCount;
        set => this.RaiseAndSetIfChanged(ref _tagCount, value);
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
    /// 标签计数显示文本
    /// </summary>
    private string _tagCountText = string.Empty;

    public string TagCountText
    {
        get => _tagCountText;
        set => this.RaiseAndSetIfChanged(ref _tagCountText, value);
    }

    /// <summary>
    /// 标签项目显示文本
    /// </summary>
    private string _taggedProjectsText = string.Empty;

    public string TaggedProjectsText
    {
        get => _taggedProjectsText;
        set => this.RaiseAndSetIfChanged(ref _taggedProjectsText, value);
    }

    #endregion

    #region 侧边栏树形数据

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

    #region 构造函数与初始化

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IProjectAppService projectAppService,
        IWorkFolderAppService workFolderAppService,
        IWorkSpaceAppService workSpaceAppService,
        IDialogService dialogService,
        IThemeService themeService,
        IScheduler mainThreadScheduler,
        IServiceProvider serviceProvider)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workFolderAppService = workFolderAppService;
        _workSpaceAppService = workSpaceAppService;
        _dialogService = dialogService;
        _themeService = themeService;
        _serviceProvider = serviceProvider;

        RefreshCommand = CreateCommand(RefreshAsync);

        CreateFolderCommand = ReactiveCommand.CreateFromTask<TreeItemViewModel>(CreateFolderAsync);
        DeleteFolderCommand = ReactiveCommand.CreateFromTask<TreeItemViewModel>(DeleteFolderAsync);
        AddContentCommand = ReactiveCommand.CreateFromTask(AddContentAsync);

        // 初始化语言和主题切换命令
        ToggleLanguageCommand = ReactiveCommand.CreateFromTask(ToggleLanguageAsync);
        ToggleThemeCommand = ReactiveCommand.Create(_themeService.ToggleTheme);

        // 订阅命令异常，防止未处理的异常导致 ReactiveUI 报错
        RefreshCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "刷新命令发生错误"));
        CreateFolderCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "创建文件夹时发生错误"));
        ToggleLanguageCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换语言时发生错误"));

        // 订阅语言切换事件，更新测试文本（确保在 UI 线程执行）
        L.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                BuildSidebarTree(); // 语言变化时重建树形结构
                UpdateStatistics(); // 语言变化时更新状态栏文本
            })
            .DisposeWith(Disposables);

        // 订阅 MessageBus 消息
        SubscribeToMessageBus();

        // 首次加载：只加载项目和文件夹（用于显示最近使用和工作文件夹）
        _ = LoadInitialDataAsync();

        // 订阅 SelectedTreeItem 变化，更新右侧内容
        this.WhenAnyValue(x => x.SelectedTreeItem)
            .Where(item => item != null)
            .Subscribe(item => UpdateContentItems(item!))
            .DisposeWith(Disposables);
    }

    #endregion

    #region MessageBus 消息处理

    /// <summary>
    /// 订阅 MessageBus 消息
    /// </summary>
    private void SubscribeToMessageBus()
    {
        // 项目相关消息
        MessageBus.Current.Listen<ProjectEditRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectEditRequested(msg.Project))
            .DisposeWith(Disposables);

        MessageBus.Current.Listen<ProjectDeleteRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectDeleteRequested(msg.Project))
            .DisposeWith(Disposables);

        MessageBus.Current.Listen<ProjectFavoriteChangedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectFavoriteChanged(msg.Project))
            .DisposeWith(Disposables);

        MessageBus.Current.Listen<ProjectLaunchedMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnProjectLaunched(msg.Project))
            .DisposeWith(Disposables);

        // 工作空间相关消息
        MessageBus.Current.Listen<WorkSpaceEditRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnWorkSpaceEditRequested(msg.WorkSpace))
            .DisposeWith(Disposables);

        MessageBus.Current.Listen<WorkSpaceDeleteRequestMessage>()
            .ObserveOn(MainThreadScheduler)
            .Subscribe(msg => OnWorkSpaceDeleteRequested(msg.WorkSpace))
            .DisposeWith(Disposables);

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

    #endregion

    #region 数据加载方法

    /// <summary>
    /// 首次加载数据（加载项目和文件夹）
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        // 加载项目数据（用于显示最近使用、收藏夹等）
        await LoadProjectsAsync();

        // 加载工作文件夹用于构建侧边栏树结构
        //todo 懒加载
        await LoadWorkFoldersAsync();

        // 加载工作空间数据（用于显示侧边栏工作空间数量）
        await LoadWorkSpacesAsync();

        // 更新统计
        UpdateStatistics();

        // 构建侧边栏树形结构（此时 Projects 和 WorkSpaces 已有数据）
        BuildSidebarTree();

        // 默认选中"最近使用"
        if (SidebarTreeItems.Count > 0)
        {
            SelectedTreeItem = SidebarTreeItems.First(); // Recent
        }
    }


    private void UpdateStatistics()
    {
        TotalProjectCount = Projects.Count;
        FavoriteProjectCount = Projects.Count(p => p.IsFavorite);
        TotalFolderCount = WorkFolders.Count;
        TotalWorkspaceCount = WorkSpaces.Count;

        // 更新格式化后的状态栏文本（支持语言切换）
        ProjectCountText = string.Format(L.Status_ProjectCount, TotalProjectCount);
        FolderCountText = string.Format(L.Status_FolderCount, TotalFolderCount);
        WorkspaceCountText = string.Format(L.Status_WorkspaceCount, TotalWorkspaceCount);
        TagCountText = string.Format(L.Status_TagCount, TagCount);
        TaggedProjectsText = string.Format(L.Status_TaggedProjects, FavoriteProjectCount);
    }

    /// <summary>
    /// 加载所有项目
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        try
        {
            Logger.LogInformation("开始加载项目列表");

            var projects = await _projectAppService.GetAllActiveAsync();

            Projects.Clear();
            foreach (var project in projects)
            {
                var projectVm = new ProjectViewModel(project, _projectAppService);
                Projects.Add(projectVm);
            }

            Logger.LogInformation($"成功加载 {projects.Count} 个项目");

            // 注意：不要在这里调用 UpdateContentItems，避免循环调用
            // 数据加载后，UpdateContentItems 会继续执行显示内容
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载项目列表时发生错误");
        }
    }

    /// <summary>
    /// 刷新所有数据
    /// </summary>
    private async Task RefreshAsync()
    {
        try
        {
            Logger.LogInformation("开始刷新所有数据");

            // 刷新工作文件夹（用于侧边栏树）
            await LoadWorkFoldersAsync();

            // 根据当前选中的节点刷新对应数据
            if (SelectedTreeItem != null)
            {
                UpdateContentItems(SelectedTreeItem);
            }

            UpdateStatistics();

            Logger.LogInformation("数据刷新完成");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "刷新数据时发生错误");
        }
    }

    #endregion

    #region 辅助方法

    private async Task LoadWorkFoldersAsync()
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

    private async Task LoadWorkSpacesAsync()
    {
        try
        {
            Logger.LogInformation("加载工作空间列表");

            var workSpaces = await _workSpaceAppService.GetAllWithProjectCountAsync();

            WorkSpaces.Clear();
            foreach (var workSpace in workSpaces)
            {
                var workSpaceVm = new WorkSpaceViewModel(workSpace, _workSpaceAppService);
                WorkSpaces.Add(workSpaceVm);
            }

            Logger.LogInformation($"成功加载 {workSpaces.Count} 个工作空间");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载工作空间列表时发生错误");
        }
    }

    #endregion

    #region 文件夹管理方法

    /// <summary>
    /// 构建侧边栏树形结构
    ///
    /// DDD 设计要点:
    /// - 纯业务逻辑，不包含 UI 操作
    /// - 响应式调用，当数据或语言变化时自动重建
    /// - 符合跨平台设计要求
    /// </summary>
    private void BuildSidebarTree()
    {
        if (SidebarTreeItems == null)
            return;

        SidebarTreeItems.Clear();

        // Recent (最近使用) - 包含项目和工作空间
        var recentProjectCount = Projects.Count(p => p.LastOpenedAt.HasValue);
        var recentWorkSpaceCount = WorkSpaces.Count(w => w.LastOpenedAt.HasValue);
        var recentCount = recentProjectCount + recentWorkSpaceCount;
        var recent = new TreeItemViewModel(L.Sidebar_Recent, recentCount, TreeItemType.RecentProject)
        { IsSelected = true };
        SidebarTreeItems.Add(recent);

        // Favorites (收藏夹) - 包含项目和工作空间
        var favoriteProjectCount = Projects.Count(p => p.IsFavorite);
        var favoriteWorkSpaceCount = WorkSpaces.Count(w => w.IsFavorite);
        var favoriteCount = favoriteProjectCount + favoriteWorkSpaceCount;
        var favorites = new TreeItemViewModel(L.Sidebar_Favorites, favoriteCount, TreeItemType.FavoriteProject);
        SidebarTreeItems.Add(favorites);
        // Workspaces (工作空间)
        var workspaces = new TreeItemViewModel(L.Sidebar_Workspaces, WorkSpaces.Count, TreeItemType.WorkSpace);
        SidebarTreeItems.Add(workspaces);
        // Work Folders (工作文件夹) - 支持树形结构
        var folderNodes = WorkFolders
            .OrderBy(f => f.SortOrder)
            .ToDictionary(f => f.Id, f => new TreeItemViewModel(f.Name, f.TotalCount, TreeItemType.WorkFolder, f.Id));
        foreach (var folder in WorkFolders.OrderBy(f => f.SortOrder))
        {
            if (folder.ParentId.HasValue && folderNodes.TryGetValue(folder.ParentId.Value, out var parentNode))
            {
                // 有父节点，添加到父节点的 Children 中
                parentNode.Children.Add(folderNodes[folder.Id]);
            }
            else
            {
                // 根节点，直接添加到侧边栏
                SidebarTreeItems.Add(folderNodes[folder.Id]);
            }
        }

        // All Projects (所有项目)
        var allProjects = new TreeItemViewModel(L.Sidebar_AllProjects, Projects.Count, TreeItemType.AllProjects);
        SidebarTreeItems.Add(allProjects);

        // Tag Settings (标签设置) - 暂时使用固定数量，后续从 TagAppService 获取
        var tagSettings = new TreeItemViewModel(L.Sidebar_TagSettings, 0, TreeItemType.TagSettings);
        SidebarTreeItems.Add(tagSettings);

        Logger.LogDebug("侧边栏树形结构已重建");
    }

    /// <summary>
    /// 创建文件夹
    ///
    /// DDD 设计要点:
    /// - 通过 IDialogService 显示对话框（支持跨平台）
    /// - 调用应用服务完成业务逻辑
    /// - 成功后刷新列表并重建树形结构
    /// </summary>
    private async Task CreateFolderAsync(TreeItemViewModel parent)
    {
        try
        {
            IsLoading = true;
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
        finally
        {
            IsLoading = false;
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

            IsLoading = true;

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
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region 项目/工作空间添加方法

    private async Task AddContentAsync()
    {
        try
        {
            if (SelectedTreeItem == null) return;

            switch (SelectedTreeItem.ItemType)
            {
                case TreeItemType.AllProjects:
                    await AddProjectAsync();
                    break;

                case TreeItemType.WorkSpace:
                    await AddWorkSpaceAsync();
                    break;

                case TreeItemType.WorkFolder:
                    // 在工作文件夹下添加项目（可以扩展）
                    await AddProjectToFolderAsync(SelectedTreeItem.Id);
                    break;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync(
                L.Message_SaveFailed,
                ex.Message);
        }
        finally
        {
            IsLoading = false;
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
            IsLoading = true;

            Logger.LogInformation("项目创建成功: {ProjectName}", result.Value.Name);

            // 直接添加新项目到集合，无需重新查询数据库
            var projectVm = new ProjectViewModel(result.Value, _projectAppService);
            Projects.Add(projectVm);
            var projectTreeView = this.SidebarTreeItems.Where(r => r.ItemType == TreeItemType.AllProjects).First();
            projectTreeView.Count++;
            // 刷新内容区域显示（如果当前选中的是项目相关节点）
            if (SelectedTreeItem != null &&
                (SelectedTreeItem.ItemType == TreeItemType.AllProjects ||
                 SelectedTreeItem.ItemType == TreeItemType.RecentProject ||
                 SelectedTreeItem.ItemType == TreeItemType.FavoriteProject))
            {
                UpdateContentItems(SelectedTreeItem);
            }

            // 更新统计
            UpdateStatistics();

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
            IsLoading = true;
            Logger.LogInformation("工作空间创建成功: {WorkSpaceName}", result.Value.Name);

            // 直接添加新工作空间到集合，无需重新查询数据库
            var workSpaceVm = new WorkSpaceViewModel(result.Value, _workSpaceAppService);
            WorkSpaces.Add(workSpaceVm);

            // 更新侧边栏工作空间节点计数
            var workSpaceTreeView = this.SidebarTreeItems.Where(r => r.ItemType == TreeItemType.WorkSpace).First();
            workSpaceTreeView.Count++;

            // 刷新内容区域显示（如果当前选中的是工作空间节点）
            if (SelectedTreeItem?.ItemType == TreeItemType.WorkSpace)
            {
                UpdateContentItems(SelectedTreeItem);
            }

            // 更新统计
            UpdateStatistics();

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
            IsLoading = true;

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

    #region 项目消息处理方法

    /// <summary>
    /// 处理项目编辑请求
    /// </summary>
    private async void OnProjectEditRequested(ProjectViewModel projectVm)
    {
        try
        {
            Logger.LogInformation($"打开编辑项目对话框: {projectVm.Name}");

            var dialogViewModel = _serviceProvider.GetRequiredService<ProjectDialogViewModel>();
            dialogViewModel.InitializeForEdit(projectVm.GetProjectDto());

            var result = await _dialogService.ShowDialogAsync<ProjectDialogViewModel, ProjectDto?>(dialogViewModel);

            if (result.Confirmed && result.Value != null)
            {
                Logger.LogInformation($"项目编辑成功: {result.Value.Name}");

                // 显示成功提示
                _dialogService.ShowNotification(
                    string.Format(L.Message_ProjectUpdated, result.Value.Name),
                    NotificationType.Success,
                    3000);
            }
            else
            {
                Logger.LogInformation("用户取消编辑项目");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "编辑项目时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_SaveFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// 处理项目删除请求
    /// </summary>
    private async void OnProjectDeleteRequested(ProjectViewModel projectVm)
    {
        try
        {
            Logger.LogInformation($"请求删除项目: {projectVm.Name}");

            // 显示确认对话框
            var confirmed = await _dialogService.ShowConfirmAsync(
                L.DeleteConfirm_Title,
                string.Format(L.DeleteConfirm_Message, projectVm.Name));

            if (!confirmed)
            {
                Logger.LogInformation("用户取消删除项目");
                return;
            }

            IsLoading = true;

            // 执行删除
            await _projectAppService.DeleteAsync(projectVm.Id);

            Logger.LogInformation($"项目删除成功: {projectVm.Name}");

            // 从列表中移除
            Projects.Remove(projectVm);

            // 更新侧边栏树形节点计数
            UpdateSidebarTreeCountsAfterProjectDelete(projectVm);

            // 刷新内容区域
            if (SelectedTreeItem != null)
            {
                UpdateContentItems(SelectedTreeItem);
            }

            // 更新统计
            UpdateStatistics();

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_ProjectDeleted, projectVm.Name),
                NotificationType.Success,
                3000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除项目时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_DeleteFailed,
                ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 删除项目后更新侧边栏树形节点计数
    /// </summary>
    private void UpdateSidebarTreeCountsAfterProjectDelete(ProjectViewModel projectVm)
    {
        foreach (var item in SidebarTreeItems)
        {
            switch (item.ItemType)
            {
                case TreeItemType.AllProjects:
                    item.Count--;
                    break;
                case TreeItemType.RecentProject:
                    if (projectVm.LastOpenedAt.HasValue)
                        item.Count--;
                    break;
                case TreeItemType.FavoriteProject:
                    if (projectVm.IsFavorite)
                        item.Count--;
                    break;
            }

            // 递归更新文件夹计数
            UpdateFolderCountsRecursive(item.Children, projectVm.Id, isProject: true);
        }
    }

    /// <summary>
    /// 递归更新文件夹计数
    /// </summary>
    private void UpdateFolderCountsRecursive(ObservableCollection<TreeItemViewModel> children, long itemId, bool isProject)
    {
        foreach (var child in children)
        {
            if (child.ItemType == TreeItemType.WorkFolder)
            {
                // 文件夹计数减1（因为删除了一个项目或工作空间）
                child.Count--;
            }

            // 递归处理子节点
            if (child.Children.Count > 0)
            {
                UpdateFolderCountsRecursive(child.Children, itemId, isProject);
            }
        }
    }

    #endregion

    #region 语言与主题方法

    /// <summary>
    /// 处理项目收藏状态变化
    /// </summary>
    private void OnProjectFavoriteChanged(ProjectViewModel projectVm)
    {
        // 刷新侧边栏树（收藏数量变化）
        BuildSidebarTree();

        // 如果当前选中的是收藏夹，刷新内容区域
        if (SelectedTreeItem?.ItemType == TreeItemType.FavoriteProject)
        {
            UpdateContentItems(SelectedTreeItem);
        }

        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 处理项目启动（启动次数更新）
    /// </summary>
    private void OnProjectLaunched(ProjectViewModel projectVm)
    {
        // 如果当前选中的是最近使用，刷新内容区域（排序可能变化）
        if (SelectedTreeItem?.ItemType == TreeItemType.RecentProject)
        {
            UpdateContentItems(SelectedTreeItem);
        }

        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    private async Task ToggleLanguageAsync()
    {
        var currentCulture = L.CurrentCulture;
        var newCulture = currentCulture.Name == "zh-CN"
            ? new System.Globalization.CultureInfo("en-US")
            : new System.Globalization.CultureInfo("zh-CN");

        await L.SetCultureAsync(newCulture);

        // 刷新侧边栏树节点名称以应用新语言
        RefreshSidebarTreeNames();

        Logger.LogInformation("语言已切换至: {Culture}", newCulture.Name);
    }

    #endregion

    #region 内容更新方法

    /// <summary>
    /// 刷新侧边栏树节点名称
    /// </summary>
    private void RefreshSidebarTreeNames()
    {
        // 更新特殊节点的名称
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
            if (SelectedTreeItem.ItemType == item.ItemType && SelectedTreeItem.ItemType != TreeItemType.WorkFolder)
            {
                SelectedTreeItem.Name = item.Name;
            }
        }
    }

    /// <summary>
    /// 根据选中的树节点更新内容项
    /// </summary>
    private async void UpdateContentItems(TreeItemViewModel selectedItem)
    {
        ContentItems.Clear();
        switch (selectedItem.ItemType)
        {
            case TreeItemType.AllProjects:
                // 显示所有项目
                foreach (var project in Projects)
                {
                    ContentItems.Add(project);
                }
                break;

            case TreeItemType.RecentProject:
                // 显示最近使用的项目和工作空间（混合按最后打开时间排序）
                var recentItems = Projects
                    .Where(p => p.LastOpenedAt.HasValue)
                    .Select(p => (object)p)
                    .Concat(WorkSpaces.Where(w => w.LastOpenedAt.HasValue).Select(w => (object)w))
                    .OrderByDescending(item => item is ProjectViewModel p ? p.LastOpenedAt : ((WorkSpaceViewModel)item).LastOpenedAt)
                    .ToList();
                foreach (var item in recentItems)
                {
                    ContentItems.Add(item);
                }
                break;

            case TreeItemType.FavoriteProject:
                // 显示收藏的项目和工作空间（混合按收藏时间排序）
                var favoriteItems = Projects
                    .Where(p => p.IsFavorite)
                    .Select(p => (object)p)
                    .Concat(WorkSpaces.Where(w => w.IsFavorite).Select(w => (object)w))
                    .OrderByDescending(item => item is ProjectViewModel p ? p.FavoritedAt : ((WorkSpaceViewModel)item).FavoritedAt)
                    .ToList();
                foreach (var item in favoriteItems)
                {
                    ContentItems.Add(item);
                }
                break;

            case TreeItemType.WorkSpace:
                // 显示所有工作空间
                foreach (var workSpace in WorkSpaces)
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
                    var projectVm = new ProjectViewModel(project, _projectAppService);
                    ContentItems.Add(projectVm);
                }

                // 添加工作空间
                foreach (var workSpace in folderWorkSpaces)
                {
                    var workSpaceVm = new WorkSpaceViewModel(workSpace, _workSpaceAppService);
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

    #region 工作空间消息处理方法

    /// <summary>
    /// 处理工作空间编辑请求
    /// </summary>
    private async void OnWorkSpaceEditRequested(WorkSpaceViewModel workSpaceVm)
    {
        try
        {
            Logger.LogInformation($"打开编辑工作空间对话框: {workSpaceVm.Name}");

            var dialogViewModel = _serviceProvider.GetRequiredService<WorkSpaceDialogViewModel>();
            await dialogViewModel.InitializeForEditAsync(workSpaceVm.Id);

            var result = await _dialogService.ShowDialogAsync<WorkSpaceDialogViewModel, WorkSpaceDto?>(dialogViewModel);

            if (result.Confirmed && result.Value != null)
            {
                // 更新工作空间的项目计数
                workSpaceVm.UpdateProjectCount(result.Value.ProjectCount);
                Logger.LogInformation($"工作空间编辑成功: {workSpaceVm.Name}");
                // 显示成功提示
                _dialogService.ShowNotification(
                    string.Format(L.Message_WorkSpaceUpdated, workSpaceVm.Name),
                    NotificationType.Success,
                    3000);
            }
            else
            {
                Logger.LogInformation("用户取消编辑工作空间");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "编辑工作空间时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_SaveFailed,
                ex.Message);
        }
    }

    /// <summary>
    /// 处理工作空间删除请求
    /// </summary>
    private async void OnWorkSpaceDeleteRequested(WorkSpaceViewModel workSpaceVm)
    {
        try
        {
            Logger.LogInformation($"请求删除工作空间: {workSpaceVm.Name}");

            // 显示确认对话框
            var confirmed = await _dialogService.ShowConfirmAsync(
                L.DeleteConfirm_Title,
                string.Format(L.DeleteConfirm_Message, workSpaceVm.Name));

            if (!confirmed)
            {
                Logger.LogInformation("用户取消删除工作空间");
                return;
            }

            IsLoading = true;

            // 执行删除
            await _workSpaceAppService.DeleteAsync(workSpaceVm.Id);

            Logger.LogInformation($"工作空间删除成功: {workSpaceVm.Name}");

            // 从列表中移除
            WorkSpaces.Remove(workSpaceVm);

            // 更新侧边栏树形节点计数
            UpdateSidebarTreeCountsAfterWorkSpaceDelete(workSpaceVm);

            // 刷新内容区域
            if (SelectedTreeItem != null)
            {
                UpdateContentItems(SelectedTreeItem);
            }

            // 更新统计
            UpdateStatistics();

            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_WorkSpaceDeleted, workSpaceVm.Name),
                NotificationType.Success,
                3000);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除工作空间时发生错误");
            await _dialogService.ShowMessageAsync(
                L.Message_DeleteFailed,
                ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 删除工作空间后更新侧边栏树形节点计数
    /// </summary>
    private void UpdateSidebarTreeCountsAfterWorkSpaceDelete(WorkSpaceViewModel workSpaceVm)
    {
        foreach (var item in SidebarTreeItems)
        {
            switch (item.ItemType)
            {
                case TreeItemType.WorkSpace:
                    item.Count--;
                    break;
                case TreeItemType.RecentProject:
                    if (workSpaceVm.LastOpenedAt.HasValue)
                        item.Count--;
                    break;
                case TreeItemType.FavoriteProject:
                    if (workSpaceVm.IsFavorite)
                        item.Count--;
                    break;
            }

            // 递归更新文件夹计数
            UpdateFolderCountsRecursive(item.Children, workSpaceVm.Id, isProject: false);
        }
    }

    /// <summary>
    /// 处理工作空间收藏状态变化
    /// </summary>
    private void OnWorkSpaceFavoriteChanged(WorkSpaceViewModel workSpaceVm)
    {
        // 刷新侧边栏树（收藏数量变化）
        BuildSidebarTree();

        // 如果当前选中的是收藏夹，刷新内容区域
        if (SelectedTreeItem?.ItemType == TreeItemType.FavoriteProject)
        {
            UpdateContentItems(SelectedTreeItem);
        }

        // 更新统计
        UpdateStatistics();
    }

    #endregion

    #region 移动到文件夹方法

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
    /// 显示文件夹选择器并执行移动操作（支持懒加载）
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

            // 只加载根级文件夹（懒加载子文件夹）
            var rootFolders = await _workFolderAppService.GetRootFoldersAsync();
            var folderItems = rootFolders.Select(f => new FolderTreeItemViewModel(f.Id, f.Name, f.ParentId)
            {
                FullPath = f.Name
            }).ToList();
            dialogVm.LoadFolders(folderItems);

            // 显示对话框
            var result = await _dialogService.ShowDialogAsync(dialogVm);

            if (!result)
            {
                Logger.LogInformation("用户取消移动到文件夹");
                return;
            }

            // 刷新侧边栏树（文件夹数量变化）
            await LoadWorkFoldersAsync();
            BuildSidebarTree();

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
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

}

/// <summary>
/// 视图模式枚举
/// </summary>
public enum ViewMode
{
    List,   // 列表视图
    Card    // 卡片视图
}