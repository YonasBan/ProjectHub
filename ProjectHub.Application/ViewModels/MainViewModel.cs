using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
    // 注入应用服务
    private readonly IProjectAppService _projectAppService;
    private readonly IServiceProvider _serviceProvider;

    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly IDialogService _dialogService;
    private readonly IThemeService _themeService;

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

    // ========== Reactive Commands ==========

    /// <summary>
    /// 加载项目命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadProjectsCommand { get; }

    /// <summary>
    /// 搜索项目命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> SearchProjectsCommand { get; }

    /// <summary>
    /// 刷新数据命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    /// <summary>
    /// 创建文件夹命令
    /// </summary>
    public ReactiveCommand<TreeItemViewModel, Unit> CreateFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> AddContentCommand { get; }

    /// <summary>
    /// 切换语言命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleLanguageCommand { get; }

    /// <summary>
    /// 切换主题命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }

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

    // ========== 侧边栏统计属性 ==========

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

    // ========== 状态栏格式化文本 (用于语言切换) ==========

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

    // ========== 侧边栏树形数据 ==========

    /// <summary>
    /// 侧边栏树形节点集合
    /// </summary>
    private ObservableCollection<TreeItemViewModel>? _sidebarTreeItems;

    public ObservableCollection<TreeItemViewModel> SidebarTreeItems
    {
        get => _sidebarTreeItems ??= new();
        set => this.RaiseAndSetIfChanged(ref _sidebarTreeItems, value);
    }

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

        // 初始化命令 (使用方法的分组语法)
        LoadProjectsCommand = CreateCommand(LoadProjectsAsync);
        SearchProjectsCommand = CreateCommand(SearchProjectsAsync);
        RefreshCommand = CreateCommand(RefreshAsync);

        CreateFolderCommand = ReactiveCommand.CreateFromTask<TreeItemViewModel>(
            CreateFolderAsync);
        AddContentCommand = ReactiveCommand.CreateFromTask(AddContentAsync);

        // 初始化语言和主题切换命令
        ToggleLanguageCommand = ReactiveCommand.CreateFromTask(ToggleLanguageAsync);
        ToggleThemeCommand = ReactiveCommand.Create(_themeService.ToggleTheme);

        // 订阅命令异常，防止未处理的异常导致 ReactiveUI 报错
        LoadProjectsCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "加载项目命令发生错误"));
        SearchProjectsCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "搜索项目命令发生错误"));
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
        // 订阅数据集合变化，自动重建树形结构
        Projects.CollectionChanged += (_, _) => BuildSidebarTree();
        WorkFolders.CollectionChanged += (_, _) => BuildSidebarTree();
        WorkSpaces.CollectionChanged += (_, _) => BuildSidebarTree();
        _ = LoadAllDataAsync();

        // 延迟设置默认选中项，等待树形结构构建完成
        this.WhenAnyValue(x => x.SidebarTreeItems)
            .Where(items => items != null && items.Count > 0)
            .Take(1)
            .Subscribe(_ => SelectedTreeItem = SidebarTreeItems.First());
    }


    private async Task LoadAllDataAsync()
    {
        await LoadProjectsAsync();
        await LoadWorkFoldersAsync();
        await LoadWorkSpacesAsync();
        UpdateStatistics();

        // 初始加载时构建侧边栏树形结构
        BuildSidebarTree();
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
                projectVm.EditRequested += OnProjectEditRequested;
                projectVm.DeleteRequested += OnProjectDeleteRequested;
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
    /// 搜索项目
    /// </summary>
    private async Task SearchProjectsAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                await LoadProjectsAsync();
                return;
            }

            Logger.LogInformation($"搜索关键字：{SearchKeyword}");

            var projects = await _projectAppService.SearchAsync(SearchKeyword);

            Projects.Clear();
            foreach (var project in projects)
            {
                var projectVm = new ProjectViewModel(project, _projectAppService);
                projectVm.EditRequested += OnProjectEditRequested;
                projectVm.DeleteRequested += OnProjectDeleteRequested;
                Projects.Add(projectVm);
            }

            Logger.LogInformation($"找到 {projects.Count} 个匹配的项目");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "搜索项目时发生错误");
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

            await LoadProjectsAsync();
            await LoadWorkFoldersAsync();
            await LoadWorkSpacesAsync();
            UpdateStatistics();

            Logger.LogInformation("数据刷新完成");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "刷新数据时发生错误");
        }
    }

    // ========== 辅助方法 ==========

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
                WorkSpaces.Add(new WorkSpaceViewModel(workSpace));
            }

            Logger.LogInformation($"成功加载 {workSpaces.Count} 个工作空间");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载工作空间列表时发生错误");
        }
    }

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

        // Recent (最近使用)
        var recentCount = Projects.Count(p => p.LastOpenedAt.HasValue);
        var recent = new TreeItemViewModel(L.Sidebar_Recent, recentCount, TreeItemType.RecentProject)
        { IsSelected = true };
        SidebarTreeItems.Add(recent);

        // Favorites (收藏夹)
        var favoriteCount = Projects.Count(p => p.IsFavorite);
        var favorites = new TreeItemViewModel(L.Sidebar_Favorites, favoriteCount, TreeItemType.FavoriteProject);
        SidebarTreeItems.Add(favorites);

        // Workspaces (工作空间)
        var workspaces = new TreeItemViewModel(L.Sidebar_Workspaces, WorkSpaces.Count, TreeItemType.WorkSpace);
        foreach (var ws in WorkSpaces)
        {
            workspaces.Children.Add(new TreeItemViewModel(ws.Name, ws.ProjectCount, TreeItemType.WorkSpace, ws.Id));
        }
        SidebarTreeItems.Add(workspaces);

        // Work Folders (工作文件夹) - 支持树形结构
        var folderNodes = WorkFolders
            .OrderBy(f => f.SortOrder)
            .ToDictionary(f => f.Id, f => new TreeItemViewModel(f.Name, f.ProjectCount, TreeItemType.WorkFolder, f.Id));

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

                // 刷新文件夹列表
                await LoadWorkFoldersAsync();

                // 更新统计
                UpdateStatistics();
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
            
            // 刷新项目列表
            await LoadProjectsAsync();
            
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
        var result = await _dialogService.ShowDialogAsync<WorkSpaceDialogViewModel, bool>(viewModel);

        if (result.Confirmed && result.Value)
        {
            IsLoading = true;
            
            Logger.LogInformation("工作空间创建成功");
            
            // 刷新工作空间列表
            await LoadWorkSpacesAsync();
            
            // 更新统计
            UpdateStatistics();
            
            // 显示成功提示
            _dialogService.ShowNotification(
                string.Format(L.Message_WorkSpaceCreated, viewModel.WorkSpaceName),
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
            
            // 刷新项目列表
            await LoadProjectsAsync();
            
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
    /// 处理项目编辑请求
    /// </summary>
    private async void OnProjectEditRequested(object? sender, ProjectViewModel projectVm)
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
                
                // 更新项目列表
                await LoadProjectsAsync();
                
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
    private async void OnProjectDeleteRequested(object? sender, ProjectViewModel projectVm)
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
        }
    }

}

/// <summary>
/// 视图模式枚举
/// </summary>
public enum ViewMode
{
    List,   // 列表视图
    Card    // 卡片视图
}