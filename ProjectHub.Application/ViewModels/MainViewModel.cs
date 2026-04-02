using Microsoft.Extensions.Logging;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
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
    // TODO: 注入应用服务
    private readonly IProjectAppService _projectAppService;
    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly ILocalizationService _localizationService;

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
    /// 切换到中文命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToChineseCommand { get; }

    /// <summary>
    /// 切换到英文命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToEnglishCommand { get; }

    /// <summary>
    /// 当前语言显示文本
    /// </summary>
    private string _currentLanguageText = string.Empty;
    public string CurrentLanguageText
    {
        get => _currentLanguageText;
        set => this.RaiseAndSetIfChanged(ref _currentLanguageText, value);
    }

    /// <summary>
    /// 测试文本 (用于验证本地化)
    /// </summary>
    private string _testText = string.Empty;
    public string TestText
    {
        get => _testText;
        set => this.RaiseAndSetIfChanged(ref _testText, value);
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
        ILocalizationService localizationService,
        IScheduler mainThreadScheduler)
        : base(logger, mainThreadScheduler)
    {
        _projectAppService = projectAppService;
        _workFolderAppService = workFolderAppService;
        _workSpaceAppService = workSpaceAppService;
        _localizationService = localizationService;

        // 初始化命令 (使用方法的分组语法)
        LoadProjectsCommand = CreateCommand(LoadProjectsAsync);
        SearchProjectsCommand = CreateCommand(SearchProjectsAsync);
        RefreshCommand = CreateCommand(RefreshAsync);
        SwitchToChineseCommand = ReactiveCommand.CreateFromTask(
            SwitchToChineseAsync,
            outputScheduler: MainThreadScheduler);
        SwitchToEnglishCommand = ReactiveCommand.CreateFromTask(
            SwitchToEnglishAsync,
            outputScheduler: MainThreadScheduler);

        // 订阅命令异常，防止未处理的异常导致 ReactiveUI 报错
        LoadProjectsCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "加载项目命令发生错误"));
        SearchProjectsCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "搜索项目命令发生错误"));
        RefreshCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "刷新命令发生错误"));
        SwitchToChineseCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换到中文时发生错误"));
        SwitchToEnglishCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换到英文时发生错误"));

        // 订阅语言切换事件，更新测试文本（确保在 UI 线程执行）
        _localizationService.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                UpdateTestText();
                BuildSidebarTree(); // 语言变化时重建树形结构
                UpdateStatistics(); // 语言变化时更新状态栏文本
            })
            .DisposeWith(Disposables);

        // 初始化测试文本
        UpdateTestText();

        // 订阅数据集合变化，自动重建树形结构
        Projects.CollectionChanged += (_, _) => BuildSidebarTree();
        WorkFolders.CollectionChanged += (_, _) => BuildSidebarTree();
        WorkSpaces.CollectionChanged += (_, _) => BuildSidebarTree();
        _ = LoadAllDataAsync();
    }

    private async Task LoadAllDataAsync()
    {
        await LoadProjectsAsync();
        await LoadWorkFoldersAsync();
        await LoadWorkSpacesAsync();
        UpdateStatistics();
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
    /// 更新测试文本
    /// </summary>
    private void UpdateTestText()
    {
        TestText = _localizationService["Project_Create"];
        CurrentLanguageText = _localizationService.CurrentCulture.Name;
    }

    /// <summary>
    /// 切换到中文（确保在 UI 线程执行）
    /// </summary>
    private async Task SwitchToChineseAsync()
    {
        await _localizationService.SetCultureAsync(new CultureInfo("zh-CN"));
    }

    /// <summary>
    /// 切换到英文（确保在 UI 线程执行）
    /// </summary>
    private async Task SwitchToEnglishAsync()
    {
        await _localizationService.SetCultureAsync(new CultureInfo("en-US"));
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
                Projects.Add(new ProjectViewModel(project));
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
                Projects.Add(new ProjectViewModel(project));
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

        // Work Folders (工作文件夹)
        foreach (var folder in WorkFolders)
        {
            var folderNode = new TreeItemViewModel(folder.Name, folder.ProjectCount, TreeItemType.WorkFolder, folder.Id);
            SidebarTreeItems.Add(folderNode);
        }

        // All Projects (所有项目)
        var allProjects = new TreeItemViewModel(L.Sidebar_AllProjects, Projects.Count, TreeItemType.AllProjects);
        SidebarTreeItems.Add(allProjects);

        // Tag Settings (标签设置) - 暂时使用固定数量，后续从 TagAppService 获取
        var tagSettings = new TreeItemViewModel(L.Sidebar_TagSettings, 0, TreeItemType.TagSettings);
        SidebarTreeItems.Add(tagSettings);

        Logger.LogDebug("侧边栏树形结构已重建");
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
