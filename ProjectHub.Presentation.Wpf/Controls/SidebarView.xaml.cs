using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.Localization;
using ProjectHub.Presentation.Wpf.Models;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class SidebarView : UserControl
{
    private ObservableCollection<TreeItemViewModel> _treeItems = new();
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    public LocalizedStrings L => _l;

    public SidebarView()
    {
        InitializeComponent();
        InitializeTreeData();
    }

    private void InitializeTreeData()
    {
        // Recent
        var recent = new TreeItemViewModel(L.Sidebar_Recent, "\uD83D\uDD52", 12, isSpecial: true)
        { IsSelected = true };
        _treeItems.Add(recent);

        // Favorites
        var favorites = new TreeItemViewModel(L.Sidebar_Favorites, "\u2B50", 5, isSpecial: true);
        _treeItems.Add(favorites);

        // Workspaces
        var workspaces = new TreeItemViewModel(L.Sidebar_Workspaces, "", 8, isSpecial: true);
        _treeItems.Add(workspaces);

        // Dev Projects (expanded with children)
        var devProjects = new TreeItemViewModel(L.Sidebar_DevProjects, "\uD83D\uDCC1", 12)
        { IsExpanded = true };
        devProjects.Children.Add(new TreeItemViewModel(L.Sidebar_WebApp, "\uD83D\uDCC1", 5));
        devProjects.Children.Add(new TreeItemViewModel(L.Sidebar_MobileApp, "\uD83D\uDCC1", 4));
        devProjects.Children.Add(new TreeItemViewModel(L.Sidebar_DesktopApp, "\uD83D\uDCC1", 3));
        _treeItems.Add(devProjects);

        // Learning Resources (expanded with children)
        var learning = new TreeItemViewModel(L.Sidebar_Learning, "\uD83D\uDCC1", 8)
        { IsExpanded = true };
        learning.Children.Add(new TreeItemViewModel(L.Sidebar_Tutorials, "\uD83D\uDCC1", 3));
        learning.Children.Add(new TreeItemViewModel(L.Sidebar_EBooks, "\uD83D\uDCC1", 5));
        _treeItems.Add(learning);

        // All Projects
        var allProjects = new TreeItemViewModel(L.Sidebar_AllProjects, "\uD83D\uDCE6", 45);
        _treeItems.Add(allProjects);

        // Tag Settings
        var tagSettings = new TreeItemViewModel(L.Sidebar_TagSettings, "\uD83C\uDFF7\uFE0F", 8, isSpecial: true);
        _treeItems.Add(tagSettings);
    }

    private void NavigationTree_OnLoaded(object sender, RoutedEventArgs e)
    {
        NavigationTree.ItemsSource = _treeItems;
    }

    private void ItemBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border { Tag: TreeItemViewModel item })
            item.IsHovered = true;
    }

    private void ItemBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border { Tag: TreeItemViewModel item })
            item.IsHovered = false;
    }

    private void ItemBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border { Tag: TreeItemViewModel item })
        {
            ClearSelection(_treeItems);
            item.IsSelected = true;

            if (item.Children.Count > 0)
                item.IsExpanded = !item.IsExpanded;
        }
    }

    private static void ClearSelection(ObservableCollection<TreeItemViewModel> items)
    {
        foreach (var item in items)
        {
            item.IsSelected = false;
            ClearSelection(item.Children);
        }
    }
}
