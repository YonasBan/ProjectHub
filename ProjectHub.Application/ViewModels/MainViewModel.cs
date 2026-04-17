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


    #region 构造函数与初始化

    public MainViewModel(
        ILogger<MainViewModel> logger,
        IThemeService themeService,
        IScheduler mainThreadScheduler,
        IServiceProvider serviceProvider,
        IDialogService dialogService,
        IWorkFolderAppService workFolderAppService,
        SidebarViewModel sidebarViewModel,
        ContentViewModel contentViewModel)
        : base(logger, mainThreadScheduler)
    {
        _themeService = themeService;
        _serviceProvider = serviceProvider;
        _dialogService = dialogService;
        _workFolderAppService = workFolderAppService;
        SidebarViewModel = sidebarViewModel;
        ContentViewModel = contentViewModel;

        // 初始化语言和主题切换命令
        ToggleLanguageCommand = ReactiveCommand.CreateFromTask(ToggleLanguageAsync);
        ToggleThemeCommand = ReactiveCommand.Create(_themeService.ToggleTheme);
        ToggleLanguageCommand.ThrownExceptions.Subscribe(ex => Logger.LogError(ex, "切换语言时发生错误"));

        // 订阅语言切换事件
        L.CultureChanged
            .ObserveOn(MainThreadScheduler)
            .Subscribe(_ =>
            {
                SidebarViewModel.BuildSidebarTree();
                SidebarViewModel.UpdateStatistics();
            })
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
    /// 首次加载数据
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        await SidebarViewModel.LoadInitialDataAsync();
    }

    #endregion

    #region 语言与主题方法

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
            await SidebarViewModel.LoadWorkFoldersAsync();
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