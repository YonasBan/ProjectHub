using ProjectHub.Presentation.Wpf.Models;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// 图标路径映射器 - 将 TreeItemType 映射到对应的图标资源
/// </summary>
public static class IconPathMapper
{
    // 图标资源路径
    private const string IconBasePath = "/Icons/";

    /// <summary>
    /// 根据项目类型获取图标路径
    /// </summary>
    public static string GetIconPath(TreeItemType itemType, bool isExpanded = false)
    {
        return itemType switch
        {
            TreeItemType.RecentProject => $"{IconBasePath}lateuse.svg",
            TreeItemType.FavoriteProject => $"{IconBasePath}favorites.svg",
            TreeItemType.WorkFolder => $"{IconBasePath}folder.svg",
            TreeItemType.WorkSpace => $"{IconBasePath}workspace.svg",
            TreeItemType.AllProjects => $"{IconBasePath}project.svg",
            TreeItemType.TagSettings => $"{IconBasePath}tags.svg",
            _ => $"{IconBasePath}project.svg"
        };
    }

    /// <summary>
    /// 获取搜索图标路径
    /// </summary>
    public static string SearchIconPath => $"{IconBasePath}search.svg";
}