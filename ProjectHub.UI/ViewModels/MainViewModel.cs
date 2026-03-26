using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace ProjectHub.UI.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// 
/// ⚠️ 注意：此文件仅为框架示例，实际实现需要在第二阶段完成
/// </summary>
public class MainViewModel : ViewModelBase
{
    // TODO: 注入应用服务
    // private readonly IProjectAppService _projectAppService;
    // private readonly IGroupAppService _groupAppService;
    // private readonly ITagAppService _tagAppService;

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
    /// 分组列表 (Observable)
    /// </summary>
    private ObservableCollection<GroupViewModel>? _groups;
    public ObservableCollection<GroupViewModel> Groups
    {
        get => _groups ??= new();
        set => this.RaiseAndSetIfChanged(ref _groups, value);
    }

    /// <summary>
    /// 标签列表 (Observable)
    /// </summary>
    private ObservableCollection<TagViewModel>? _tags;
    public ObservableCollection<TagViewModel> Tags
    {
        get => _tags ??= new();
        set => this.RaiseAndSetIfChanged(ref _tags, value);
    }

    /// <summary>
    /// 当前选中的分组
    /// </summary>
    private GroupViewModel? _selectedGroup;
    public GroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set => this.RaiseAndSetIfChanged(ref _selectedGroup, value);
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

    public MainViewModel(ILogger<MainViewModel> logger)
        : base(logger)
    {
        Title = "Project Hub - 开发者项目管理器";
        
        // 初始化命令 (使用方法的分组语法)
        LoadProjectsCommand = CreateCommand(LoadProjectsAsync);
        SearchProjectsCommand = CreateCommand(SearchProjectsAsync);
        RefreshCommand = CreateCommand(RefreshAsync);
        
        // TODO: 加载数据
        // LoadProjectsCommand.Execute(Unit.Default).Subscribe();
    }

    /// <summary>
    /// 加载所有项目
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        await ExecuteWithErrorHandlerAsync(async () =>
        {
            // TODO: 调用应用服务加载数据
            // var projects = await _projectAppService.GetAllActiveAsync();
            // Projects = new ObservableCollection<ProjectViewModel>(
            //     projects.Select(p => new ProjectViewModel(p))
            // );
            
            Logger.LogInformation("加载了 {Count} 个项目", Projects.Count);
        }, "加载项目");
    }

    /// <summary>
    /// 搜索项目
    /// </summary>
    private async Task SearchProjectsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            await LoadProjectsAsync();
            return;
        }

        await ExecuteWithErrorHandlerAsync(async () =>
        {
            // TODO: 调用搜索服务
            // var results = await _projectAppService.SearchAsync(SearchKeyword);
            // Projects = new ObservableCollection<ProjectViewModel>(
            //     results.Select(p => new ProjectViewModel(p))
            // );
        }, "搜索项目");
    }

    /// <summary>
    /// 刷新所有数据
    /// </summary>
    private async Task RefreshAsync()
    {
        await LoadProjectsAsync();
        await LoadGroupsAsync();
        await LoadTagsAsync();
    }

    // ========== 辅助方法 ==========

    private async Task LoadGroupsAsync()
    {
        // TODO: 实现
        await Task.CompletedTask;
    }

    private async Task LoadTagsAsync()
    {
        // TODO: 实现
        await Task.CompletedTask;
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
