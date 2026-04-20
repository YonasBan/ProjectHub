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
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsProject { get; set; }
}

public class SelectFolderDialogViewModel : DialogViewModelBase
{
    private readonly ObservableCollection<FolderTreeItemViewModel> _folders;
    private FolderTreeItemViewModel? _selectedFolder;
    private string _newFolderName = string.Empty;
    private bool _isCreatingNewFolder;
    private string? _errorMessage;

    public ObservableCollection<FolderTreeItemViewModel> Folders => _folders;

    public FolderTreeItemViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set => this.RaiseAndSetIfChanged(ref _selectedFolder, value);
    }

    public bool IsCreatingNewFolder
    {
        get => _isCreatingNewFolder;
        set => this.RaiseAndSetIfChanged(ref _isCreatingNewFolder, value);
    }

    public string NewFolderName
    {
        get => _newFolderName;
        set => this.RaiseAndSetIfChanged(ref _newFolderName, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateNewFolderCommand { get; }

    public string Title { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public MoveItemInfo? MoveItem { get; set; }

    private readonly IWorkFolderAppService _workFolderAppService;
    private ObservableCollection<TreeItemViewModel>? _sidebarTreeItems;

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

        ConfirmCommand = ReactiveCommand.Create(() => Close(true), canConfirm);
        CancelCommand = ReactiveCommand.Create(() => Close(false));

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

    public void LoadFoldersFromSidebar(ObservableCollection<TreeItemViewModel> sidebarTreeItems)
    {
        _sidebarTreeItems = sidebarTreeItems;
        _folders.Clear();

        var folderItems = ExtractFolderTreeFromSidebar(sidebarTreeItems);
        foreach (var folder in folderItems)
        {
            _folders.Add(folder);
        }
    }

    private List<FolderTreeItemViewModel> ExtractFolderTreeFromSidebar(ObservableCollection<TreeItemViewModel> sidebarTreeItems)
    {
        var folderItems = new List<FolderTreeItemViewModel>();

        foreach (var item in sidebarTreeItems)
        {
            if (item.ItemType == TreeItemType.WorkFolder)
            {
                var folderItem = ConvertTreeItemToFolderTreeItem(item, null);
                if (folderItem != null)
                {
                    folderItems.Add(folderItem);
                }
            }
        }

        return folderItems;
    }

    private FolderTreeItemViewModel? ConvertTreeItemToFolderTreeItem(TreeItemViewModel treeItem, string? parentFullPath)
    {
        if (treeItem.ItemType != TreeItemType.WorkFolder)
            return null;

        var fullPath = parentFullPath == null ? treeItem.Name : $"{parentFullPath}/{treeItem.Name}";

        var folderItem = new FolderTreeItemViewModel(treeItem.Id, treeItem.Name, null)
        {
            FullPath = fullPath
        };

        foreach (var child in treeItem.Children)
        {
            var childItem = ConvertTreeItemToFolderTreeItem(child, fullPath);
            if (childItem != null)
            {
                folderItem.Children.Add(childItem);
            }
        }

        return folderItem;
    }

    public async Task<bool> ConfirmAsync()
    {
        if (MoveItem == null)
        {
            ErrorMessage = L?.Dialog_MoveItemNotSet ?? "未设置要移动的项目";
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
                var createDto = new CreateWorkFolderDto
                {
                    Name = NewFolderName.Trim(),
                    SortOrder = 0,
                    ParentId = SelectedFolder?.Id
                };

                var newFolder = await _workFolderAppService.CreateAsync(createDto);

                if (MoveItem.IsProject)
                {
                    await _workFolderAppService.AddProjectToFolderAsync(MoveItem.Id, newFolder.Id);
                }
                else
                {
                    await _workFolderAppService.AddWorkSpaceToFolderAsync(MoveItem.Id, newFolder.Id);
                }

                if (_sidebarTreeItems != null)
                {
                    AddNewFolderToSidebarTree(newFolder, SelectedFolder?.Id);
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

    public long? GetSelectedFolderId() => IsCreatingNewFolder ? null : SelectedFolder?.Id;

    public string? GetNewFolderName() => IsCreatingNewFolder ? NewFolderName.Trim() : null;

    private void AddNewFolderToSidebarTree(WorkFolderDto newFolder, long? parentId)
    {
        if (_sidebarTreeItems == null) return;

        var newTreeItem = new TreeItemViewModel(newFolder.Name, 1, TreeItemType.WorkFolder, newFolder.Id);

        if (parentId.HasValue)
        {
            var parentNode = FindParentNodeInSidebar(_sidebarTreeItems, parentId.Value);
            parentNode?.Children.Add(newTreeItem);
        }
        else
        {
            var workSpaceIndex = -1;
            for (int i = 0; i < _sidebarTreeItems.Count; i++)
            {
                if (_sidebarTreeItems[i].ItemType == TreeItemType.WorkSpace)
                {
                    workSpaceIndex = i;
                    break;
                }
            }
            _sidebarTreeItems.Insert(workSpaceIndex + 1, newTreeItem);
        }
    }

    private TreeItemViewModel? FindParentNodeInSidebar(ObservableCollection<TreeItemViewModel> nodes, long parentId)
    {
        foreach (var node in nodes)
        {
            if (node.Id == parentId && node.ItemType == TreeItemType.WorkFolder)
            {
                return node;
            }

            var foundInChildren = FindParentNodeInSidebar(node.Children, parentId);
            if (foundInChildren != null)
            {
                return foundInChildren;
            }
        }
        return null;
    }
}

public class FolderTreeItemViewModel : ReactiveObject
{
    private bool _isSelected;

    public long Id { get; }
    public string Name { get; }
    public long? ParentId { get; }
    public ObservableCollection<FolderTreeItemViewModel> Children { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string FullPath { get; set; } = string.Empty;

    public FolderTreeItemViewModel(long id, string name, long? parentId = null)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
        Children = new ObservableCollection<FolderTreeItemViewModel>();
    }
}
