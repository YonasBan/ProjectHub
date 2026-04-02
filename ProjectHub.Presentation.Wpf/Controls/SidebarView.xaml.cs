using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using ProjectHub.Presentation.Wpf.Models;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class SidebarView : UserControl
{
    private ObservableCollection<TreeItemViewModel> _treeItems = new();
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    private readonly ILocalizationService? _localizationService = Locator.Current.GetService<ILocalizationService>();
    public LocalizedStrings L => _l;

    public SidebarView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        
        // 订阅语言改变事件
        _localizationService?.CultureChanged.Subscribe(_ => RefreshTreeData());
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Projects.CollectionChanged += (_, _) => RefreshTreeData();
            vm.WorkFolders.CollectionChanged += (_, _) => RefreshTreeData();
            vm.WorkSpaces.CollectionChanged += (_, _) => RefreshTreeData();
            RefreshTreeData();
        }
    }

    private void RefreshTreeData()
    {
        if (DataContext is not MainViewModel vm) return;

        _treeItems.Clear();

        // Recent (最近使用)
        var recentCount = vm.Projects.Count(p => p.LastOpenedAt.HasValue);
        var recent = new TreeItemViewModel(L.Sidebar_Recent, "\uD83D\uDD52", recentCount, TreeItemType.RecentProject)
        { IsSelected = true };
        _treeItems.Add(recent);

        // Favorites (收藏夹)
        var favoriteCount = vm.Projects.Count(p => p.IsFavorite);
        var favorites = new TreeItemViewModel(L.Sidebar_Favorites, "\u2B50", favoriteCount, TreeItemType.FavoriteProject);
        _treeItems.Add(favorites);

        // Workspaces (工作空间)
        var workspaces = new TreeItemViewModel(L.Sidebar_Workspaces, "", vm.WorkSpaces.Count, TreeItemType.WorkSpace);
        foreach (var ws in vm.WorkSpaces)
        {
            workspaces.Children.Add(new TreeItemViewModel(ws.Name, "\uD83D\uDCC1", ws.ProjectCount, TreeItemType.WorkSpace, ws.Id));
        }
        _treeItems.Add(workspaces);

        // Work Folders (工作文件夹)
        foreach (var folder in vm.WorkFolders)
        {
            var folderNode = new TreeItemViewModel(folder.Name, "\uD83D\uDCC1", folder.ProjectCount, TreeItemType.WorkFolder, folder.Id);
            _treeItems.Add(folderNode);
        }

        // All Projects (所有项目)
        var allProjects = new TreeItemViewModel(L.Sidebar_AllProjects, "\uD83D\uDCE6", vm.Projects.Count, TreeItemType.AllProjects);
        _treeItems.Add(allProjects);

        // Tag Settings (标签设置) - 暂时使用固定数量，后续从 TagAppService 获取
        var tagSettings = new TreeItemViewModel(L.Sidebar_TagSettings, "\uD83C\uDFF7\uFE0F", 0, TreeItemType.TagSettings);
        _treeItems.Add(tagSettings);

        // 刷新 TreeView
        if (NavigationTree.ItemsSource != _treeItems)
        {
            NavigationTree.ItemsSource = _treeItems;
        }
    }

    private void NavigationTree_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            RefreshTreeData();
        }
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
