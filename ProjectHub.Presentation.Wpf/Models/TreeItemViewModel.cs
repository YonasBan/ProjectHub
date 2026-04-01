using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace ProjectHub.Presentation.Wpf.Models;

public class TreeItemViewModel : ReactiveObject
{
    public TreeItemViewModel(string name, string icon, int count, bool isSpecial = false)
    {
        Name = name;
        Icon = icon;
        Count = count;
        IsSpecial = isSpecial;
        Children = new ObservableCollection<TreeItemViewModel>();
    }

    public string Name { get; }
    public string Icon { get; }
    public int Count { get; }
    public bool IsSpecial { get; }

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
