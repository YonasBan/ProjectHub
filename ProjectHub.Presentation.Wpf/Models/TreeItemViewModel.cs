using ProjectHub.Presentation.Wpf.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace ProjectHub.Presentation.Wpf.Models;

public enum TreeItemType
{
    Special,
    RecentProject,
    FavoriteProject,
    WorkFolder,
    WorkSpace,
    AllProjects,
    TagSettings
}

public class TreeItemViewModel : ReactiveObject
{
    public TreeItemViewModel(string name,  int count, TreeItemType itemType = TreeItemType.Special, long id = 0)
    {
        Name = name;
        Count = count;
        ItemType = itemType;
        Id = id;
        IconPath = IconPathMapper.GetIconPath(itemType);
        Children = new ObservableCollection<TreeItemViewModel>();
    }

    public long Id { get; }
    public string Name { get; }
    public string IconPath { get; }
    public int Count { get; set; }
    public bool IsSpecial => ItemType == TreeItemType.Special;
    public TreeItemType ItemType { get; }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    private bool _isHovered;
    public bool IsHovered
    {
        get => _isHovered;
        set => this.RaiseAndSetIfChanged(ref _isHovered, value);
    }

    public ObservableCollection<TreeItemViewModel> Children { get; }

    public ReactiveCommand<Unit, Unit>? AddCommand { get; set; }
    public ReactiveCommand<Unit, Unit>? DeleteCommand { get; set; }
}
