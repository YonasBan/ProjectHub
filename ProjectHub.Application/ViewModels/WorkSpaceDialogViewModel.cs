using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Concurrency;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 工作空间对话框 ViewModel
/// 支持添加和编辑工作空间
/// </summary>
public class WorkSpaceDialogViewModel : DialogViewModelBase<bool>
{
    private readonly IWorkSpaceAppService _workSpaceAppService;
    private readonly IProjectAppService _projectAppService;
    private readonly IScheduler mainScheduler;

    // ========== 对话框状态 ==========
    private bool _isEditMode;
    private long? _workSpaceId;

    // ========== 表单字段 ==========
    private string _workSpaceName = string.Empty;
    private string? _description;

    // ========== 项目选择 ==========
    private readonly SourceList<SelectableProjectViewModel> _sourceList = new();
    private readonly ReadOnlyObservableCollection<SelectableProjectViewModel> _filteredProjects;
    private string _searchText = string.Empty;

    // ========== 启动配置 ==========
    private int _defaultLaunchIntervalSeconds;
    private bool _useCustomLaunchOrder;

    // ========== 错误处理 ==========
    private string? _errorMessage;
    private bool _hasError;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    /// <summary>
    /// 是否为编辑模式
    /// </summary>
    public bool IsEditMode
    {
        get => _isEditMode;
        private set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    /// <summary>
    /// 对话框标题
    /// </summary>
    public string DialogTitle => IsEditMode ? L.WorkSpaceDialog_Title_Edit : L.WorkSpaceDialog_Title_Add;

    /// <summary>
    /// 确认按钮文本
    /// </summary>
    public string ConfirmButtonText => IsEditMode ? L.Save : L.Add;

    /// <summary>
    /// 工作空间名称
    /// </summary>
    public string WorkSpaceName
    {
        get => _workSpaceName;
        set => this.RaiseAndSetIfChanged(ref _workSpaceName, value);
    }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// 搜索文本
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    /// <summary>
    /// 默认启动间隔时间（秒）
    /// </summary>
    public int DefaultLaunchIntervalSeconds
    {
        get => _defaultLaunchIntervalSeconds;
        set => this.RaiseAndSetIfChanged(ref _defaultLaunchIntervalSeconds, value);
    }

    /// <summary>
    /// 是否使用自定义启动顺序
    /// </summary>
    public bool UseCustomLaunchOrder
    {
        get => _useCustomLaunchOrder;
        set => this.RaiseAndSetIfChanged(ref _useCustomLaunchOrder, value);
    }

    /// <summary>
    /// 可选项目列表（带选择状态）
    /// </summary>
    public ReadOnlyObservableCollection<SelectableProjectViewModel> AvailableProjects => _filteredProjects;

    /// <summary>
    /// 已选择的项目数量
    /// </summary>
    public int SelectedProjectCount => _sourceList.Items.Count(p => p.IsSelected);

    /// <summary>
    /// 是否有选择项目
    /// </summary>
    public bool HasSelectedProjects => SelectedProjectCount > 0;

    /// <summary>
    /// 确认命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    /// <summary>
    /// 上移项目命令
    /// </summary>
    public ReactiveCommand<SelectableProjectViewModel, Unit> MoveUpCommand { get; }

    /// <summary>
    /// 下移项目命令
    /// </summary>
    public ReactiveCommand<SelectableProjectViewModel, Unit> MoveDownCommand { get; }

    public WorkSpaceDialogViewModel(
        IWorkSpaceAppService workSpaceAppService,
        IProjectAppService projectAppService,IScheduler mainScheduler)
    {
        _workSpaceAppService = workSpaceAppService;
        _projectAppService = projectAppService;
        this.mainScheduler = mainScheduler;

        // 初始化命令
        var canConfirm = this.WhenAnyValue(
            x => x.WorkSpaceName,
            name => !string.IsNullOrWhiteSpace(name));

        ConfirmCommand = ReactiveCommand.CreateFromTask(ConfirmAsync, canConfirm);
        MoveUpCommand = ReactiveCommand.Create<SelectableProjectViewModel>(MoveUp);
        MoveDownCommand = ReactiveCommand.Create<SelectableProjectViewModel>(MoveDown);

        // 构建过滤管道
        var filterPredicate = this
            .WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(150))
            .DistinctUntilChanged()
            .ObserveOn(mainScheduler)
            .Select(BuildFilter);

        _sourceList
            .Connect()
            .Filter(filterPredicate)
            .ObserveOn(mainScheduler)
            .Bind(out _filteredProjects)
            .Subscribe();

        // 监听选择变化，更新计数
        _sourceList
            .Connect()
            .WhenPropertyChanged(p => p.IsSelected)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(SelectedProjectCount));
                this.RaisePropertyChanged(nameof(HasSelectedProjects));
            });
    }

    /// <summary>
    /// 构建过滤条件
    /// </summary>
    private static Func<SelectableProjectViewModel, bool> BuildFilter(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return _ => true;

        var lower = keyword.ToLowerInvariant();
        return item =>
            item.ProjectName.ToLowerInvariant().Contains(lower) ||
            item.ProjectPath.ToLowerInvariant().Contains(lower);
    }

    /// <summary>
    /// 初始化为添加模式
    /// </summary>
    public async Task InitializeForAddAsync(CancellationToken cancellationToken = default)
    {
        IsEditMode = false;
        _workSpaceId = null;
        WorkSpaceName = string.Empty;
        Description = null;
        DefaultLaunchIntervalSeconds = 0;
        UseCustomLaunchOrder = false;

        await LoadProjectsAsync(cancellationToken);
    }

    /// <summary>
    /// 初始化为编辑模式
    /// </summary>
    public async Task InitializeForEditAsync(long workSpaceId, CancellationToken cancellationToken = default)
    {
        IsEditMode = true;
        _workSpaceId = workSpaceId;

        var workSpace = await _workSpaceAppService.GetByIdAsync(workSpaceId, cancellationToken);
        if (workSpace == null)
        {
            throw new KeyNotFoundException($"WorkSpace (Id={workSpaceId}) not found");
        }

        WorkSpaceName = workSpace.Name;
        Description = workSpace.Description;
        DefaultLaunchIntervalSeconds = workSpace.DefaultLaunchIntervalSeconds;
        UseCustomLaunchOrder = workSpace.UseCustomLaunchOrder;

        await LoadProjectsAsync(cancellationToken, workSpace.EnabledProjectIds);
    }

    /// <summary>
    /// 加载项目列表
    /// </summary>
    private async Task LoadProjectsAsync(CancellationToken cancellationToken, IReadOnlyList<long>? selectedIds = null)
    {
        var projects = await _projectAppService.GetAllActiveAsync(cancellationToken);
        var selectedIdSet = selectedIds?.ToHashSet() ?? new HashSet<long>();

        var viewModels = projects
            .Select((p, index) => new SelectableProjectViewModel
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                ProjectPath = p.Path,
                ProjectType = p.Type.ToString(),
                IconPath = !string.IsNullOrEmpty(p.CustomIconPath) ? p.CustomIconPath : p.Path,
                IsSelected = selectedIdSet.Contains(p.Id),
                SortOrder = index
            })
            .ToList();

        _sourceList.Clear();
        _sourceList.AddRange(viewModels);
    }

    /// <summary>
    /// 确认保存
    /// </summary>
    private async Task ConfirmAsync()
    {
        try
        {
            var selectedProjectIds = _sourceList.Items
                .Where(p => p.IsSelected)
                .Select(p => p.ProjectId)
                .ToList();

            if (IsEditMode && _workSpaceId.HasValue)
            {
                // 更新工作空间
                var updateDto = new UpdateWorkSpaceDto
                {
                    Id = _workSpaceId.Value,
                    Name = WorkSpaceName.Trim(),
                    Description = Description
                };
                await _workSpaceAppService.UpdateAsync(updateDto);

                // 更新项目设置
                var settingsDto = new UpdateWorkSpaceProjectSettingsDto
                {
                    WorkSpaceId = _workSpaceId.Value,
                    EnabledProjectIds = selectedProjectIds,
                    UseCustomLaunchOrder = UseCustomLaunchOrder,
                    DefaultLaunchIntervalSeconds = DefaultLaunchIntervalSeconds
                };
                await _workSpaceAppService.UpdateProjectSettingsAsync(settingsDto);
            }
            else
            {
                // 创建工作空间
                var createDto = new CreateWorkSpaceDto
                {
                    Name = WorkSpaceName.Trim(),
                    Description = Description,
                    SortOrder = 0
                };
                var workSpace = await _workSpaceAppService.CreateAsync(createDto);

                // 设置项目
                if (selectedProjectIds.Any())
                {
                    var settingsDto = new UpdateWorkSpaceProjectSettingsDto
                    {
                        WorkSpaceId = workSpace.Id,
                        EnabledProjectIds = selectedProjectIds,
                        UseCustomLaunchOrder = UseCustomLaunchOrder,
                        DefaultLaunchIntervalSeconds = DefaultLaunchIntervalSeconds
                    };
                    await _workSpaceAppService.UpdateProjectSettingsAsync(settingsDto);
                }
            }

            Close(true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }


    /// <summary>
    /// 上移项目（仅在自定义顺序模式下有效）
    /// </summary>
    private void MoveUp(SelectableProjectViewModel project)
    {
        if (!UseCustomLaunchOrder) return;

        var list = _sourceList.Items.ToList();
        var index = list.IndexOf(project);
        if (index > 0)
        {
            list.RemoveAt(index);
            list.Insert(index - 1, project);
            _sourceList.Clear();
            _sourceList.AddRange(list);
            UpdateSortOrders();
        }
    }

    /// <summary>
    /// 下移项目（仅在自定义顺序模式下有效）
    /// </summary>
    private void MoveDown(SelectableProjectViewModel project)
    {
        if (!UseCustomLaunchOrder) return;

        var list = _sourceList.Items.ToList();
        var index = list.IndexOf(project);
        if (index < list.Count - 1)
        {
            list.RemoveAt(index);
            list.Insert(index + 1, project);
            _sourceList.Clear();
            _sourceList.AddRange(list);
            UpdateSortOrders();
        }
    }

    /// <summary>
    /// 更新排序序号
    /// </summary>
    private void UpdateSortOrders()
    {
        var list = _sourceList.Items.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            list[i].SortOrder = i;
        }
    }
}

/// <summary>
/// 可选择的项目 ViewModel
/// </summary>
public class SelectableProjectViewModel : ReactiveObject
{
    private bool _isSelected;
    private int _sortOrder;

    public long ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
    public string? IconPath { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public int SortOrder
    {
        get => _sortOrder;
        set => this.RaiseAndSetIfChanged(ref _sortOrder, value);
    }
}
