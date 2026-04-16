using ProjectHub.Application.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace ProjectHub.Application.ViewModels;

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

    public SelectFolderDialogViewModel()
    {
        _folders = new ObservableCollection<FolderTreeItemViewModel>();

        var canConfirm = this.WhenAnyValue(
            x => x.SelectedFolder,
            x => x.IsCreatingNewFolder,
            x => x.NewFolderName,
            (selected, isCreating, newName) => 
                selected != null || (isCreating && !string.IsNullOrWhiteSpace(newName)));

        ConfirmCommand = ReactiveCommand.Create(() =>
        {
            if (IsCreatingNewFolder)
            {
                if (string.IsNullOrWhiteSpace(NewFolderName))
                {
                    ErrorMessage = L?.Folder_NameRequired ?? "文件夹名称不能为空";
                    return;
                }
            }
            else if (SelectedFolder == null)
            {
                ErrorMessage = L?.Folder_SelectRequired ?? "请选择一个文件夹";
                return;
            }

            ErrorMessage = null;
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
    /// 加载文件夹树
    /// </summary>
    public void LoadFolders(IEnumerable<FolderTreeItemViewModel> folders)
    {
        _folders.Clear();
        foreach (var folder in folders)
        {
            _folders.Add(folder);
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
/// 文件夹树形节点 ViewModel
/// </summary>
public class FolderTreeItemViewModel : ReactiveObject
{
    private bool _isExpanded;
    private bool _isSelected;

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
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
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
}
