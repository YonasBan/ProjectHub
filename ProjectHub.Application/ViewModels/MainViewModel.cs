using DynamicData;
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
/// 主窗口 ViewModel - 管理应用程序主界面状态和交互
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

    #region 子 ViewModels

    /// <summary>
    /// 侧边栏 ViewModel
    /// </summary>
    public SidebarViewModel SidebarViewModel { get; }

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



    #region 构造函数与初始化

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IProjectAppService projectAppService,
        IWorkFolderAppService workFolderAppService,
        IWorkSpaceAppService workSpaceAppService,
        IDialogService dialogService,
        IThemeService themeService,
        IScheduler mainThreadScheduler,
        IServiceProvider serviceProvider,
        SidebarViewModel sidebarViewModel)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workFolderAppService = workFolderAppService;
        _workSpaceAppService = workSpaceAppService;
        _dialogService = dialogService;
        _themeService = themeService;
        _serviceProvider = serviceProvider;
        SidebarViewModel = sidebarViewModel;

        RefreshCommand = CreateCommand(RefreshAsync);

        AddContentCommand = ReactiveCommand.CreateFromTask(AddContentAsync);

        // 初始化语言和主题切换命令
        ToggleLanguageCommand = ReactiveCommand.CreateFromTask(ToggleLanguageAsync);
        ToggleThemeCommand = ReactiveCommand.Create(_themeService.ToggleTheme);

        // 订阅命令异常，防止未处理的异常导致 ReactiveUI 报错
        RefreshCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "刷新命令发生错误"));
        ToggleLanguageCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换语言时发生错误"));

        // 订阅语言切换事件
        L.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                SidebarViewModel.BuildSidebarTree();
                UpdateStatistics();
            })
            .DisposeWith(Disposables);

        // 订阅 MessageBus 消息
        SubscribeToMessageBus();

        // 订阅侧边栏选中项变化
        SidebarViewModel.SelectedItemChanged += OnSidebarSelectedItemChanged;

        // 首次加载
        _ = LoadInitialDataAsync();
    }

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
        // 加载项目数据
        await LoadProjectsAsync();

        // 加载工作文件夹
        await LoadWorkFoldersAsync();

        // 加载工作空间数据
        await LoadWorkSpacesAsync();

        // 更新统计
        UpdateStatistics();

        // 设置 SidebarViewModel 的数据源
        SidebarViewModel.Projects = Projects;
        SidebarViewModel.WorkSpaces = WorkSpaces;
        SidebarViewModel.WorkFolders = WorkFolders;

        // 构建侧边栏树形结构
        SidebarViewModel.BuildSidebarTree();

        // 默认选中"最近使用"
        if (SidebarViewModel.SidebarTreeItems.Count > 0)
        {
            SidebarViewModel.SelectedTreeItem = SidebarViewModel.SidebarTreeItems.First();
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

            // 刷新工作文件夹
            await LoadWorkFoldersAsync();

            // 更新 SidebarViewModel 的数据源
            SidebarViewModel.WorkFolders = WorkFolders;
            SidebarViewModel.BuildSidebarTree();

            // 根据当前选中的节点刷新对应数据
            if (SidebarViewModel.SelectedTreeItem != null)
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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



    #region 项目/工作空间添加方法

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

            // 直接添加新项目到集合
            var projectVm = new ProjectViewModel(result.Value, _projectAppService);
            Projects.Add(projectVm);
            
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

            // 直接添加新工作空间到集合
            var workSpaceVm = new WorkSpaceViewModel(result.Value, _workSpaceAppService);
            WorkSpaces.Add(workSpaceVm);

            // 更新侧边栏工作空间节点计数
            SidebarViewModel.IncrementWorkSpaceCount();

            // 刷新内容区域显示
            if (SidebarViewModel.SelectedTreeItem?.ItemType == TreeItemType.WorkSpace)
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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
            SidebarViewModel.UpdateCountsAfterDelete(projectVm);
            await LoadWorkFoldersAsync();
            SidebarViewModel.WorkFolders = WorkFolders;
            SidebarViewModel.RebuildFolderTreeOnly();

            // 刷新内容区域
            if (SidebarViewModel.SelectedTreeItem != null)
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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

    #endregion

    #region 语言与主题方法

    /// <summary>
    /// 处理项目收藏状态变化
    /// </summary>
    private void OnProjectFavoriteChanged(ProjectViewModel projectVm)
    {
        // 刷新侧边栏树
        SidebarViewModel.BuildSidebarTree();

        // 如果当前选中的是收藏夹，刷新内容区域
        if (SidebarViewModel.SelectedTreeItem?.ItemType == TreeItemType.FavoriteProject)
        {
            UpdateContentItems(SidebarViewModel.SelectedTreeItem);
        }

        // 更新统计
        UpdateStatistics();
    }

    /// <summary>
    /// 处理项目启动（启动次数更新）
    /// </summary>
    private void OnProjectLaunched(ProjectViewModel projectVm)
    {
        // 如果当前选中的是最近使用，刷新内容区域
        if (SidebarViewModel.SelectedTreeItem?.ItemType == TreeItemType.RecentProject)
        {
            UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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

        // SidebarViewModel 会自动处理语言切换
        Logger.LogInformation("语言已切换至: {Culture}", newCulture.Name);
    }

    #endregion

    #region 内容更新方法

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
                // 显示最近使用的项目和工作空间（混合按最后打开时间排序，最多20个）
                var recentItems = Projects
                    .Where(p => p.LastOpenedAt.HasValue)
                    .Select(p => (object)p)
                    .Concat(WorkSpaces.Where(w => w.LastOpenedAt.HasValue).Select(w => (object)w))
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
            SidebarViewModel.UpdateCountsAfterDelete(workSpaceVm);
            await LoadWorkFoldersAsync();
            SidebarViewModel.WorkFolders = WorkFolders;
            SidebarViewModel.RebuildFolderTreeOnly();

            // 刷新内容区域
            if (SidebarViewModel.SelectedTreeItem != null)
            {
                UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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
    /// 处理工作空间收藏状态变化
    /// </summary>
    private void OnWorkSpaceFavoriteChanged(WorkSpaceViewModel workSpaceVm)
    {
        // 刷新侧边栏树
        SidebarViewModel.BuildSidebarTree();

        // 如果当前选中的是收藏夹，刷新内容区域
        if (SidebarViewModel.SelectedTreeItem?.ItemType == TreeItemType.FavoriteProject)
        {
            UpdateContentItems(SidebarViewModel.SelectedTreeItem);
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
            SidebarViewModel.WorkFolders = WorkFolders;
            SidebarViewModel.RebuildFolderTreeOnly();

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