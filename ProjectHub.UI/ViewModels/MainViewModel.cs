using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ProjectHub.Application.Interfaces;

namespace ProjectHub.UI.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// 
/// ⚠️ 注意：此文件仅为框架示例，实际实现需要在第二阶段完成
/// </summary>
public class MainViewModel : ViewModelBase
{
    // TODO: 注入应用服务
    private readonly IProjectAppService _projectAppService;
    private readonly IGroupAppService _groupAppService;
    private readonly ITagAppService _tagAppService;

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

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IProjectAppService projectAppService,
        IGroupAppService groupAppService,
        ITagAppService tagAppService)
        : base(logger)
    {
        _projectAppService = projectAppService;
        _groupAppService = groupAppService;
        _tagAppService = tagAppService;

        // 初始化命令 (使用方法的分组语法)
        LoadProjectsCommand = CreateCommand(LoadProjectsAsync);
        SearchProjectsCommand = CreateCommand(SearchProjectsAsync);
        RefreshCommand = CreateCommand(RefreshAsync);
        
        // 加载数据
        LoadProjectsCommand.Execute(Unit.Default).Subscribe();
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
            throw;
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
            throw;
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
            await LoadGroupsAsync();
            await LoadTagsAsync();
            
            Logger.LogInformation("数据刷新完成");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "刷新数据时发生错误");
            throw;
        }
    }

    // ========== 辅助方法 ==========

    private async Task LoadGroupsAsync()
    {
        try
        {
            Logger.LogInformation("加载分组列表");
            
            var groups = await _groupAppService.GetAllWithProjectCountAsync();
            
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(new GroupViewModel(group));
            }
            
            Logger.LogInformation($"成功加载 {groups.Count} 个分组");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载分组列表时发生错误");
            throw;
        }
    }

    private async Task LoadTagsAsync()
    {
        try
        {
            Logger.LogInformation("加载标签列表");
            
            var tags = await _tagAppService.GetAllWithProjectCountAsync();
            
            Tags.Clear();
            foreach (var tag in tags)
            {
                Tags.Add(new TagViewModel(tag));
            }
            
            Logger.LogInformation($"成功加载 {tags.Count} 个标签");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载标签列表时发生错误");
            throw;
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
