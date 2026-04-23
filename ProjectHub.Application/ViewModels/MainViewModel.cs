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

    private readonly IThemeService _themeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    private readonly IWorkFolderAppService _workFolderAppService;
    private readonly IAppSettingsService _appSettingsService;

    #endregion

    #region 子 ViewModels

    /// <summary>
    /// 侧边栏 ViewModel
    /// </summary>
    public SidebarViewModel SidebarViewModel { get; }

    /// <summary>
    /// 内容区域 ViewModel
    /// </summary>
    public ContentViewModel ContentViewModel { get; }

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
    /// 设置语言命令
    /// </summary>
    public ReactiveCommand<string, Unit> SetLanguageCommand { get; }

    /// <summary>
    /// 设置主题命令
    /// </summary>
    public ReactiveCommand<string, Unit> SetThemeCommand { get; }

    #endregion

    #region 设置选项

    private ObservableCollection<CultureOption> _availableCultures = new();
    public ObservableCollection<CultureOption> AvailableCultures
    {
        get => _availableCultures;
        private set => this.RaiseAndSetIfChanged(ref _availableCultures, value);
    }

    private ObservableCollection<ThemeOption> _availableThemes = new();
    public ObservableCollection<ThemeOption> AvailableThemes
    {
        get => _availableThemes;
        private set => this.RaiseAndSetIfChanged(ref _availableThemes, value);
    }

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


    #region 构造函数与初始化

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IThemeService themeService,
        IScheduler mainThreadScheduler,
        IServiceProvider serviceProvider,
        IDialogService dialogService,
        IWorkFolderAppService workFolderAppService,
        IAppSettingsService appSettingsService,
        SidebarViewModel sidebarViewModel,
        ContentViewModel contentViewModel)
        : base(logger, mainThreadScheduler)
    {
        _themeService = themeService;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        _workFolderAppService = workFolderAppService;
        _appSettingsService = appSettingsService;
        SidebarViewModel = sidebarViewModel;
        ContentViewModel = contentViewModel;

        // 初始化语言和主题切换命令
        SetLanguageCommand = ReactiveCommand.CreateFromTask<string>(SetLanguageAsync);
        SetThemeCommand = ReactiveCommand.Create<string>(theme =>
        {
            _themeService.SetTheme(theme);
            var settings = _appSettingsService.Load();
            settings.Theme = theme;
            _appSettingsService.Save(settings);
        });
        SetLanguageCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换语言时发生错误"));

        // 初始化设置选项
        RefreshAvailableSettings();

        // 订阅语言切换事件
        L.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                SidebarViewModel.BuildSidebarTree();
                SidebarViewModel.UpdateStatistics();
                RefreshAvailableSettings();
            })
            .DisposeWith(Disposables);

        // 订阅搜索关键字变化，防抖 300ms 后触发搜索
        this.WhenAnyValue(x => x.SearchKeyword)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(MainThreadScheduler)
            .Subscribe(keyword => ContentViewModel.Search(keyword))
            .DisposeWith(Disposables);

        // 订阅 MessageBus 消息
        SubscribeToMessageBus();

        // 首次加载
        _ = LoadInitialDataAsync();
    }

    #endregion

    #region MessageBus 消息处理

    /// <summary>
    /// 订阅 MessageBus 消息
    /// </summary>
    private void SubscribeToMessageBus()
    {

    }

    #endregion

    #region 数据加载方法

    /// <summary>
    /// 首次加载数据
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        await SidebarViewModel.LoadInitialDataAsync();
    }

    #endregion

    #region 语言与主题方法

    /// <summary>
    /// 设置语言
    /// </summary>
    private async Task SetLanguageAsync(string cultureName)
    {
        var newCulture = new System.Globalization.CultureInfo(cultureName);
        await L.SetCultureAsync(newCulture);

        // 保存配置（保留其他设置）
        var settings = _appSettingsService.Load();
        settings.Language = cultureName;
        _appSettingsService.Save(settings);

        Logger.LogInformation("语言已切换至: {Culture}", newCulture.Name);
    }

    /// <summary>
    /// 刷新设置选项（语言、主题）
    /// </summary>
    private void RefreshAvailableSettings()
    {
        AvailableCultures.Clear();
        foreach (var culture in AppSettings.SupportedCultures)
        {
            AvailableCultures.Add(new CultureOption(culture, GetLanguageDisplayName(culture)));
        }

        AvailableThemes.Clear();
        AvailableThemes.Add(new ThemeOption("Light", L.Theme_Light));
        AvailableThemes.Add(new ThemeOption("Dark", L.Theme_Dark));
    }

    /// <summary>
    /// 根据语言代码获取本地化显示名称
    /// </summary>
    private string GetLanguageDisplayName(string culture)
    {
        return culture switch
        {
            "zh-CN" => L.Language_Chinese,
            "en-US" => L.Language_English,
            _ => culture
        };
    }

    #endregion
}

/// <summary>
/// 文化选项
/// </summary>
public record CultureOption(string CultureName, string DisplayName);

/// <summary>
/// 主题选项
/// </summary>
public record ThemeOption(string ThemeName, string DisplayName);

/// <summary>
/// 视图模式枚举
/// </summary>
public enum ViewMode
{
    List,   // 列表视图
    Card    // 卡片视图
}

// ========== 移动到文件夹消息 ==========

/// <summary>
/// 项目移动到文件夹请求消息
/// </summary>
public record ProjectMoveToFolderRequestMessage(ProjectViewModel Project);

/// <summary>
/// 工作空间移动到文件夹请求消息
/// </summary>
public record WorkSpaceMoveToFolderRequestMessage(WorkSpaceViewModel WorkSpace);