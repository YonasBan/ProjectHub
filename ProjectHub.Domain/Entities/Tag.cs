namespace ProjectHub.Domain.Entities;

/// <summary>
/// 标签实体
/// 用于给 Project 和 WorkSpace 添加分类标签
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 标签颜色
    /// 格式：十六进制颜色值，如 #FF5733 或 #F5733
    /// </summary>
    public string Color { get; private set; } = string.Empty;

    /// <summary>
    /// 标签描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 排序顺序
    /// 数字越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private Tag()
    {
    }

    /// <summary>
    /// 工厂方法：创建标签
    /// </summary>
    public static Tag Create(string name, string color, string? description = null, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("标签名称不能为空", nameof(name));

        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("标签颜色不能为空", nameof(color));

        // 验证颜色格式
        if (!IsValidColor(color))
            throw new ArgumentException("标签颜色格式无效，请使用十六进制格式，如 #FF5733 或 #F57", nameof(color));

        var tag = new Tag
        {
            Name = name,
            Color = color,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };

        return tag;
    }

    /// <summary>
    /// 更新标签信息
    /// </summary>
    public void UpdateInfo(string name, string color, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("标签名称不能为空", nameof(name));

        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("标签颜色不能为空", nameof(color));

        if (!IsValidColor(color))
            throw new ArgumentException("标签颜色格式无效，请使用十六进制格式，如 #FF5733 或 #F57", nameof(color));

        Name = name;
        Color = color;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新排序顺序
    /// </summary>
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 验证颜色格式是否有效
    /// 支持格式：#RGB、#RRGGBB、#RRGGBBAA
    /// </summary>
    private static bool IsValidColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // 移除可能的 # 前缀
        var hex = color.TrimStart('#');

        // 检查是否为 3、6 或 8 位十六进制数
        return hex.Length == 3 || hex.Length == 6 || hex.Length == 8
               && hex.All(char.IsLetterOrDigit);
    }
}
