using ProjectHub.Application.DTOs;
using ProjectHub.Application.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels.DialogViewModel;

/// <summary>
/// 要移动的项目/工作空间信息
/// </summary>
public class MoveItemInfo
{
    /// <summary>
    /// 项目/工作空间ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否是项目（true=项目，false=工作空间）
    /// </summary>
    public bool IsProject { get; set; }
}

/// <summary>
/// 文件夹选择对话框 ViewModel
/// </summary>
public class SelectFolderDialogViewModel : DialogViewModelBase
{
    private readonly ObservableCollection<FolderTreeItemViewModel> _folders;
    private FolderTreeItemViewModel? _selectedFolder;
    private string _newFolderName = string.Empty;
    private bool _isCreatingNewFolder;
    private string? _errorMessage;

    /// <summary>
    /// 文件夹树形结构
    /// </summary>
    public ObservableCollection<FolderTreeItemViewModel> Folders => _folders;

    /// <summary>
    /// 当前选中的文件夹
    /// </summary>
    public FolderTreeItemViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedFolder, value);
    }

    /// <summary>
    /// 是否正在创建新文件夹
    /// </summary>
    public bool IsCreatingNewFolder
    {
        get => _isCreatingNewFolder;
        set => this.RaiseAndSetIfChanged(ref _isCreatingNewFolder, value);
    }

    /// <summary>
    /// 新文件夹名称
    /// </summary>
    public string NewFolderName
    {
        get => _newFolderName;
        set => this.RaiseAndSetIfChanged(ref _newFolderName, value);
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    /// <summary>
    /// 确认命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }

    /// <summary>
    /// 取消命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    /// <summary>
    /// 创建新文件夹命令
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewFolderCommand { get; }

    /// <summary>
    /// 对话框标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 项目或工作空间名称（显示在标题中）
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 要移动的项目/工作空间信息
    /// </summary>
    public MoveItemInfo? MoveItem { get; set; }

    private readonly IWorkFolderAppService _workFolderAppService;

    public SelectFolderDialogViewModel(IWorkFolderAppService workFolderAppService)
    {
        _workFolderAppService = workFolderAppService;
        _folders = new ObservableCollection<FolderTreeItemViewModel>();

        var canConfirm = this.WhenAnyValue(
            x => x.SelectedFolder,
            x => x.IsCreatingNewFolder,
            x => x.NewFolderName,
            (selected, isCreating, newName) =>
                selected != null || (isCreating && !string.IsNullOrWhiteSpace(newName)));

        ConfirmCommand = ReactiveCommand.Create(() =>
        {
            // 确认操作移到异步方法中处理
            Close(true);
        }, canConfirm);

        CancelCommand = ReactiveCommand.Create(() =>
        {
            Close(false);
        });

        CreateNewFolderCommand = ReactiveCommand.Create(() =>
        {
            IsCreatingNewFolder = !IsCreatingNewFolder;
            if (!IsCreatingNewFolder)
            {
                NewFolderName = string.Empty;
                ErrorMessage = null;
            }
        });
    }

    /// <summary>
    /// 加载文件夹树（根级文件夹）
    /// </summary>
    public void LoadFolders(IEnumerable<FolderTreeItemViewModel> folders)
    {
        _folders.Clear();
        foreach (var folder in folders)
        {
            // 设置懒加载事件
            folder.OnExpandRequested = OnFolderExpandRequested;
            _folders.Add(folder);
        }
    }

    /// <summary>
    /// 文件夹展开时的懒加载处理
    /// </summary>
    private async void OnFolderExpandRequested(FolderTreeItemViewModel folder)
    {
        if (folder.HasLoadedChildren)
            return;

        folder.IsLoadingChildren = true;
        try
        {
            var children = await _workFolderAppService.GetChildrenAsync(folder.Id);
            var childViewModels = children.Select(c => new FolderTreeItemViewModel(c.Id, c.Name, c.ParentId)
            {
                FullPath = $"{folder.FullPath}/{c.Name}",
                OnExpandRequested = OnFolderExpandRequested
            }).ToList();

            folder.AddChildren(childViewModels);
        }
        finally
        {
            folder.IsLoadingChildren = false;
        }
    }

    /// <summary>
    /// 确认选择并执行操作
    /// </summary>
    public async Task<bool> ConfirmAsync()
    {
        if (MoveItem == null)
        {
            ErrorMessage = "未设置要移动的项目";
            return false;
        }

        if (IsCreatingNewFolder)
        {
            if (string.IsNullOrWhiteSpace(NewFolderName))
            {
                ErrorMessage = L?.Folder_NameRequired ?? "文件夹名称不能为空";
                return false;
            }

            try
            {
                // 创建新文件夹
                var createDto = new CreateWorkFolderDto
                {
                    Name = NewFolderName.Trim(),
                    SortOrder = 0,
                    ParentId = SelectedFolder?.Id
                };

                var newFolder = await _workFolderAppService.CreateAsync(createDto);

                // 将项目/工作空间添加到新文件夹
                if (MoveItem.IsProject)
                {
                    await _workFolderAppService.AddProjectToFolderAsync(MoveItem.Id, newFolder.Id);
                }
                else
                {
                    await _workFolderAppService.AddWorkSpaceToFolderAsync(MoveItem.Id, newFolder.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        else if (SelectedFolder != null)
        {
            try
            {
                if (MoveItem.IsProject)
                {
                    await _workFolderAppService.AddProjectToFolderAsync(MoveItem.Id, SelectedFolder.Id);
                }
                else
                {
                    await _workFolderAppService.AddWorkSpaceToFolderAsync(MoveItem.Id, SelectedFolder.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        else
        {
            ErrorMessage = L?.Folder_SelectRequired ?? "请选择一个文件夹";
            return false;
        }
    }

    /// <summary>
    /// 获取选中的文件夹ID（如果是创建新文件夹则返回null）
    /// </summary>
    public long? GetSelectedFolderId()
    {
        if (IsCreatingNewFolder)
            return null;
        return SelectedFolder?.Id;
    }

    /// <summary>
    /// 获取新文件夹名称（如果正在创建新文件夹）
    /// </summary>
    public string? GetNewFolderName()
    {
        if (IsCreatingNewFolder)
            return NewFolderName.Trim();
        return null;
    }
}

/// <summary>
/// 文件夹树形节点 ViewModel - 支持懒加载
/// </summary>
public class FolderTreeItemViewModel : ReactiveObject
{
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isLoadingChildren;
    private bool _hasLoadedChildren;

    public long Id { get; }
    public string Name { get; }
    public long? ParentId { get; }
    public ObservableCollection<FolderTreeItemViewModel> Children { get; }

    /// <summary>
    /// 是否展开
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            this.RaiseAndSetIfChanged(ref _isExpanded, value);
            // 展开时触发懒加载
            if (value && !_hasLoadedChildren && !_isLoadingChildren)
            {
                OnExpandRequested?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <summary>
    /// 是否正在加载子文件夹
    /// </summary>
    public bool IsLoadingChildren
    {
        get => _isLoadingChildren;
        set => this.RaiseAndSetIfChanged(ref _isLoadingChildren, value);
    }

    /// <summary>
    /// 是否已加载子文件夹
    /// </summary>
    public bool HasLoadedChildren
    {
        get => _hasLoadedChildren;
        set => this.RaiseAndSetIfChanged(ref _hasLoadedChildren, value);
    }

    /// <summary>
    /// 是否有子文件夹（用于显示展开箭头）
    /// </summary>
    public bool HasChildren => Children.Count > 0 || !HasLoadedChildren;

    /// <summary>
    /// 展开请求事件（用于懒加载）
    /// </summary>
    public Action<FolderTreeItemViewModel>? OnExpandRequested { get; set; }

    /// <summary>
    /// 完整路径显示
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    public FolderTreeItemViewModel(long id, string name, long? parentId = null)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
        Children = new ObservableCollection<FolderTreeItemViewModel>();
    }

    /// <summary>
    /// 添加子文件夹
    /// </summary>
    public void AddChildren(IEnumerable<FolderTreeItemViewModel> children)
    {
        foreach (var child in children)
        {
            Children.Add(child);
        }
        HasLoadedChildren = true;
        this.RaisePropertyChanged(nameof(HasChildren));
    }
}
