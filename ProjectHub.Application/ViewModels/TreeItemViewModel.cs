using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ProjectHub.Application.ViewModels;

/// <summary>
/// 侧边栏树形节点 ViewModel
/// 
/// DDD 设计要点:
/// - 位于 Application 层，包含 UI 展示逻辑
/// - 使用 ReactiveUI 实现跨平台兼容 (WPF/MAUI/Avalonia)
/// - 不直接引用平台特定 API
/// </summary>
public class TreeItemViewModel : ReactiveObject
{
    /// <summary>
    /// 节点唯一标识 (用于 WorkFolder/WorkSpace)
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// 节点显示名称
    /// </summary>
    private string _name;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// 图标字符 (Emoji 或 Unicode 符号)
    /// </summary>
    private string _icon;
    public string Icon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    /// <summary>
    /// 图标文件路径 (由平台层映射)
    /// </summary>
    private string _iconPath;
    public string IconPath
    {
        get => _iconPath;
        set => this.RaiseAndSetIfChanged(ref _iconPath, value);
    }

    /// <summary>
    /// 项目/文件夹数量
    /// </summary>
    private int _count;
    public int Count
    {
        get => _count;
        set => this.RaiseAndSetIfChanged(ref _count, value);
    }

    /// <summary>
    /// 是否为特殊节点 (Recent/Favorites 等)
    /// </summary>
    public bool IsSpecial => ItemType == TreeItemType.Special;

    /// <summary>
    /// 节点类型
    /// </summary>
    public TreeItemType ItemType { get; }

    /// <summary>
    /// 是否已展开
    /// </summary>
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>
    /// 是否被选中
    /// </summary>
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <summary>
    /// 是否鼠标悬停 (用于显示操作按钮)
    /// </summary>
    private bool _isHovered;
    public bool IsHovered
    {
        get => _isHovered;
        set => this.RaiseAndSetIfChanged(ref _isHovered, value);
    }

    /// <summary>
    /// 子节点集合
    /// </summary>
    public ObservableCollection<TreeItemViewModel> Children { get; }

    /// <summary>
    /// 添加子节点命令
    /// </summary>
    private ReactiveCommand<Unit, Unit>? _addCommand;
    public ReactiveCommand<Unit, Unit> AddCommand => _addCommand ??= ReactiveCommand.CreateFromTask(AddChildAsync);

    /// <summary>
    /// 删除节点命令
    /// </summary>
    private ReactiveCommand<Unit, Unit>? _deleteCommand;
    public ReactiveCommand<Unit, Unit> DeleteCommand => _deleteCommand ??= ReactiveCommand.CreateFromTask(DeleteAsync);

    /// <summary>
    /// 委托：执行添加子节点的操作
    /// </summary>
    public Func<Task>? OnAddChild { get; set; }

    /// <summary>
    /// 委托：执行删除操作
    /// </summary>
    public Func<Task>? OnDelete { get; set; }

    public TreeItemViewModel(
        string name, 
        string icon, 
        int count, 
        TreeItemType itemType = TreeItemType.Special, 
        long id = 0)
    {
        Name = name;
        Icon = icon;
        Count = count;
        ItemType = itemType;
        Id = id;
        // 使用默认的图标路径生成逻辑（基于节点类型的简单映射）
        IconPath = GetDefaultIconPath(itemType);
        Children = new ObservableCollection<TreeItemViewModel>();
    }

    /// <summary>
    /// 获取默认图标路径（Application 层的默认实现）
    /// 平台特定实现可以通过 ITreeItemIconProvider 覆盖此逻辑
    /// </summary>
    private static string GetDefaultIconPath(TreeItemType itemType)
    {
        // 这里使用 emoji 作为默认图标，平台特定的图标路径由平台层提供
        return itemType switch
        {
            TreeItemType.RecentProject => "\uD83D\uDD52", // 🕒
            TreeItemType.FavoriteProject => "\u2B50",     // ⭐
            TreeItemType.WorkFolder => "\uD83D\uDCC1",    // 📁
            TreeItemType.WorkSpace => "\uD83D\uDCC1",     // 📁
            TreeItemType.AllProjects => "\uD83D\uDCE6",   // 📦
            TreeItemType.TagSettings => "\uD83C\uDFF7\uFE0F", // 🏷️
            _ => "\uD83D\uDCC1"                           // 📁
        };
    }

    /// <summary>
    /// 更新计数
    /// </summary>
    public void UpdateCount(int newCount)
    {
        Count = newCount;
    }

    /// <summary>
    /// 更新本地化文本
    /// </summary>
    public void UpdateName(string newName)
    {
        Name = newName;
    }

    /// <summary>
    /// 添加子节点的异步实现
    /// </summary>
    private async Task AddChildAsync()
    {
        if (OnAddChild != null)
            await OnAddChild();
    }

    /// <summary>
    /// 删除节点的异步实现
    /// </summary>
    private async Task DeleteAsync()
    {
        if (OnDelete != null)
            await OnDelete();
    }
}

/// <summary>
/// 树形节点类型枚举
/// 
/// DDD 设计说明:
/// - 定义在 Application 层，因为它是业务概念而非 UI 概念
/// - 用于区分不同类型的树节点，决定图标和操作
/// - 不依赖任何平台特定 API，可在 WPF/MAUI/Avalonia 中复用
/// </summary>
public enum TreeItemType
{
    /// <summary>
    /// 特殊节点 (Recent, Favorites 等)
    /// </summary>
    Special,
    
    /// <summary>
    /// 最近项目
    /// </summary>
    RecentProject,
    
    /// <summary>
    /// 收藏项目
    /// </summary>
    FavoriteProject,
    
    /// <summary>
    /// 工作文件夹
    /// </summary>
    WorkFolder,
    
    /// <summary>
    /// 工作空间
    /// </summary>
    WorkSpace,
    
    /// <summary>
    /// 所有项目
    /// </summary>
    AllProjects,
    
    /// <summary>
    /// 标签设置
    /// </summary>
    TagSettings
}
