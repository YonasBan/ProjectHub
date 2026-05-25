using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.Interfaces;
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

    #endregion 注入服务

    #region 子 ViewModels

    /// <summary>
    /// 侧边栏 ViewModel
    /// </summary>
    public SidebarViewModel SidebarViewModel { get; }

    /// <summary>
    /// 内容区域 ViewModel
    /// </summary>
    public ContentViewModel ContentViewModel { get; }

    #endregion 子 ViewModels

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

    #endregion 搜索与视图属性

    #region Reactive Commands

    /// <summary>
    /// 设置语言命令
    /// </summary>
    public ReactiveCommand<string, Unit> SetLanguageCommand { get; }

    /// <summary>
    /// 设置主题命令
    /// </summary>
    public ReactiveCommand<string, Unit> SetThemeCommand { get; }

    #endregion Reactive Commands

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

    /// <summary>
    /// 是否最小化到托盘
    /// </summary>
    private bool _minimizeToTray;

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set
        {
            this.RaiseAndSetIfChanged(ref _minimizeToTray, value);
            // 保存配置
            var settings = _appSettingsService.Load();
            settings.MinimizeToTray = value;
            _appSettingsService.Save(settings);
            Logger.LogInformation("托盘设置已更新: {Value}", value ? "启用" : "禁用");
        }
    }

    /// <summary>
    /// 是否启用右键菜单
    /// </summary>
    private bool _enableShellContextMenu;

    public bool EnableShellContextMenu
    {
        get => _enableShellContextMenu;
        set => this.RaiseAndSetIfChanged(ref _enableShellContextMenu, value);
    }

    /// <summary>
    /// 当前选中的语言
    /// </summary>
    private string _selectedCulture;

    public string SelectedCulture
    {
        get => _selectedCulture;
        set => this.RaiseAndSetIfChanged(ref _selectedCulture, value);
    }

    /// <summary>
    /// 当前选中的主题
    /// </summary>
    private string _selectedTheme;

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
    }

    #endregion 设置选项

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
            SelectedTheme = theme;
            var settings = _appSettingsService.Load();
            settings.Theme = theme;
            _appSettingsService.Save(settings);
        });
        SetLanguageCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换语言时发生错误"));

    

        // 加载托盘设置
        var settings = _appSettingsService.Load();
        _minimizeToTray = settings.MinimizeToTray;
        _enableShellContextMenu = settings.EnableShellContextMenu;

        // 加载当前语言和主题
        SelectedCulture = settings.Language;
        SelectedTheme = settings.Theme;
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

        // 订阅右键菜单设置变化
        this.WhenAnyValue(x => x.EnableShellContextMenu)
            // 关键：Skip(1) 确保程序刚启动、第一次从配置文件加载该值时，不触发下面的注册/注销逻辑
            .Skip(1)
            // 关键：在后台线程执行耗时的 I/O 和注册表操作，绝不卡顿 UI
            .ObserveOn(System.Reactive.Concurrency.TaskPoolScheduler.Default)
            .Subscribe(value =>
            {
                // A. 保存配置
                var settings = _appSettingsService.Load();
                settings.EnableShellContextMenu = value;
                _appSettingsService.Save(settings);
                Logger.LogInformation("右键菜单设置已更新: {Value}", value ? "启用" : "禁用");

                // B. 动态注册或注销右键菜单
                var shellService = _serviceProvider.GetService<ProjectHub.Application.Interfaces.IShellContextMenuService>();
                if (shellService == null)
                {
                    Logger.LogWarning("未找到 IShellContextMenuService 服务");
                    return;
                }

                if (value)
                {
                    Logger.LogInformation("开始注册右键菜单...");
                    shellService.Register();
                    Logger.LogInformation("Windows Shell 右键菜单已注册");
                }
                else
                {
                    Logger.LogInformation("开始注销右键菜单...");
                    shellService.Unregister();
                    Logger.LogInformation("Windows Shell 右键菜单已注销");
                }
            })
            .DisposeWith(Disposables);

        // 首次加载
        _ = LoadInitialDataAsync();
    }

    #endregion 构造函数与初始化

    #region 数据加载方法

    /// <summary>
    /// 首次加载数据
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        await SidebarViewModel.LoadInitialDataAsync();
    }

    #endregion 数据加载方法

    #region 语言与主题方法

    /// <summary>
    /// 设置语言
    /// </summary>
    private async Task SetLanguageAsync(string cultureName)
    {
        var newCulture = new System.Globalization.CultureInfo(cultureName);
        await L.SetCultureAsync(newCulture);

        // 更新选中状态
        SelectedCulture = cultureName;

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
            var isSelected = culture == SelectedCulture;
            AvailableCultures.Add(new CultureOption(culture, GetLanguageDisplayName(culture), isSelected));
        }

        AvailableThemes.Clear();
        AvailableThemes.Add(new ThemeOption("Light", L.Theme_Light, "Light" == SelectedTheme));
        AvailableThemes.Add(new ThemeOption("Dark", L.Theme_Dark, "Dark" == SelectedTheme));
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

    #endregion 语言与主题方法
}

/// <summary>
/// 文化选项
/// </summary>
public class CultureOption
{
    public string CultureName { get; }
    public string DisplayName { get; }
    public bool IsSelected { get; set; }

    public CultureOption(string cultureName, string displayName, bool isSelected = false)
    {
        CultureName = cultureName;
        DisplayName = displayName;
        IsSelected = isSelected;
    }
}

/// <summary>
/// 主题选项
/// </summary>
public class ThemeOption
{
    public string ThemeName { get; }
    public string DisplayName { get; }
    public bool IsSelected { get; set; }

    public ThemeOption(string themeName, string displayName, bool isSelected = false)
    {
        ThemeName = themeName;
        DisplayName = displayName;
        IsSelected = isSelected;
    }
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