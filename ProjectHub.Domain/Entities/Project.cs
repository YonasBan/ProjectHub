
namespace ProjectHub.Domain.Entities;

/// <summary>
/// 项目实体 - 核心业务对象
/// 支持多种类型：VS 项目、Android 项目、C++ 项目、工具、文档、文件夹等
/// 
/// DDD 设计要点:
/// - 聚合根：Project 是聚合根，所有相关对象通过 ProjectId 关联
/// - 不变性：关键字段 (如 Path) 一旦设置不应直接修改，需通过领域方法
/// - 业务规则：如路径必须存在、类型必须有效等
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// 项目名称 (用户自定义)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 项目类型 (枚举值)
    /// VisualStudio, Android, Cpp, Tool, Document, Folder
    /// </summary>
    public ProjectType Type { get; private set; }

    /// <summary>
    /// 项目路径 (入口文件或根目录)
    /// 例如：.sln 文件路径、build.gradle 路径、.exe 路径、文件夹路径
    /// </summary>
    public string Path { get; private set; } = string.Empty;

    /// <summary>
    /// 自定义图标路径 (可选)
    /// 为空则使用系统默认类型图标
    /// </summary>
    public string? CustomIconPath { get; private set; }

    /// <summary>
    /// 颜色标签 (可选)
    /// 用于视觉区分，存储 ARGB 值或预定义颜色名
    /// </summary>
    public string? ColorTag { get; private set; }

    /// <summary>
    /// 备注/描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 最近打开时间
    /// 用于"最近使用"排序
    /// </summary>
    public DateTime? LastOpenedAt { get; private set; }

    /// <summary>
    /// 最后修改时间 (文件系统监听)
    /// 用于判断项目活跃度
    /// </summary>
    public DateTime? LastModifiedAt { get; private set; }

    /// <summary>
    /// 启动次数统计
    /// </summary>
    public int LaunchCount { get; private set; }

    /// <summary>
    /// 累计使用时长 (毫秒)
    /// 用于统计用户时间投入
    /// </summary>
    public long TotalUsageDurationMs { get; private set; }

    /// <summary>
    /// 是否被收藏/置顶
    /// </summary>
    public bool IsFavorite { get; private set; }

    /// <summary>
    /// 收藏/置顶时间
    /// 用于排序，数字越大越靠前
    /// </summary>
    public DateTime? FavoritedAt { get; private set; }

    /// <summary>
    /// 截止日期 (可选)
    /// 用于提醒功能
    /// </summary>
    public DateTime? Deadline { get; private set; }

    /// <summary>
    /// 关联的客户/联系人 ID (可选，后期功能)
    /// </summary>
    public long? CustomerId { get; private set; }

    /// <summary>
    /// 占用空间大小 (字节) - 懒加载字段
    /// 不直接存储在数据库，运行时计算
    /// </summary>
    public long? DiskSpaceBytes { get; private set; }

    /// <summary>
    /// 可清理空间大小 (字节) - 懒加载字段
    /// 不直接存储在数据库，运行时计算
    /// </summary>
    public long? CleanableSpaceBytes { get; private set; }

    /// <summary>
    /// 默认启动程序路径 (可选)
    /// 为空则使用系统默认关联程序
    /// </summary>
    public string? DefaultProgram { get; private set; }

    /// <summary>
    /// 启动参数 (可选)
    /// 启动程序时传递的参数
    /// </summary>
    public string? LaunchArguments { get; private set; }

    /// <summary>
    /// 网页链接 (可选)
    /// 用于 WebSite 类型项目或作为项目的相关链接
    /// </summary>
    public string? WebUrl { get; private set; }

    /// <summary>
    /// 启动类型
    /// OpenFile = 0,     // 打开文件
    /// OpenExe = 1,      // 打开 exe
    /// OpenWebUrl = 2    // 打开网页
    /// </summary>
    public LaunchType LaunchType { get; private set; } = LaunchType.OpenFile;

    /// <summary>
    /// 是否以管理员身份运行
    /// 仅对打开文件和打开 exe 模式有效
    /// </summary>
    public bool RunAsAdmin { get; private set; }

    // ========== 导航属性 ==========

    /// <summary>
    /// 关联的标签
    /// </summary>
    public ICollection<ProjectTag> ProjectTags { get; private set; } = new List<ProjectTag>();

    // ========== DDD 领域行为 ==========

    /// <summary>
    /// 私有构造函数 - 强制使用工厂方法创建
    /// </summary>
    private Project()
    {
    }

    /// <summary>
    /// 工厂方法：创建新项目
    /// 封装创建逻辑，确保初始状态合法
    /// </summary>
    public static Project Create(string name, ProjectType type, string path, string? defaultProgram, string? customIconPath, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("项目名称不能为空", nameof(name));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("项目路径不能为空", nameof(path));

        var project = new Project
        {
            Name = name,
            Type = type,
            Path = path,
            CreatedAt = DateTime.UtcNow,
            IsFavorite = false,
            LaunchCount = 0,
            TotalUsageDurationMs = 0,
            DefaultProgram = defaultProgram,
            CustomIconPath = customIconPath,
            Description = description
        };
        return project;
    }

    /// <summary>
    /// 更新项目基本信息
    /// 封装不变性规则
    /// </summary>
    public void UpdateBasicInfo(string name, string? description = null, string? colorTag = null, string? defaultProgram = null, string? launchArguments = null, string? customIconPath = null, string? webUrl = null, string? path = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("项目名称不能为空", nameof(name));

        Name = name;
        Description = description;
        ColorTag = colorTag;
        DefaultProgram = defaultProgram;
        LaunchArguments = launchArguments;
        WebUrl = webUrl;
        UpdatedAt = DateTime.UtcNow;
        CustomIconPath = customIconPath;
        
        if (!string.IsNullOrWhiteSpace(path))
        {
            Path = path;
        }
    }

    /// <summary>
    /// 设置默认启动程序
    /// </summary>
    public void SetDefaultProgram(string? defaultProgram)
    {
        DefaultProgram = defaultProgram;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置启动参数
    /// </summary>
    public void SetLaunchArguments(string? launchArguments)
    {
        LaunchArguments = launchArguments;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置网页链接
    /// </summary>
    public void SetWebUrl(string? webUrl)
    {
        WebUrl = webUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置启动类型
    /// </summary>
    public void SetLaunchType(LaunchType launchType)
    {
        LaunchType = launchType;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置是否以管理员身份运行
    /// </summary>
    public void SetRunAsAdmin(bool runAsAdmin)
    {
        RunAsAdmin = runAsAdmin;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 记录项目启动
    /// 更新统计信息
    /// </summary>
    public void RecordLaunch()
    {
        LaunchCount++;
        LastOpenedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 记录使用时长
    /// </summary>
    /// <param name="durationMs">使用时长 (毫秒)</param>
    public void RecordUsageDuration(long durationMs)
    {
        if (durationMs <= 0) return;

        TotalUsageDurationMs += durationMs;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新文件修改时间 (由文件监听服务调用)
    /// </summary>
    public void UpdateLastModifiedTime(DateTime lastModified)
    {
        LastModifiedAt = lastModified;
    }

    /// <summary>
    /// 设置收藏状态
    /// </summary>
    public void SetFavorite(bool isFavorite)
    {
        if (IsFavorite == isFavorite) return;

        IsFavorite = isFavorite;
        FavoritedAt = isFavorite ? DateTime.UtcNow : null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置磁盘空间信息 (懒加载)
    /// </summary>
    public void SetDiskSpaceInfo(long totalBytes, long cleanableBytes)
    {
        DiskSpaceBytes = totalBytes;
        CleanableSpaceBytes = cleanableBytes;
    }
}

/// <summary>
/// 项目类型枚举
/// </summary>
public enum ProjectType
{
    /// <summary>
    /// Visual Studio 项目 (.sln)
    /// </summary>
    VisualStudio = 1,

    /// <summary>
    /// Android 项目 (build.gradle)
    /// </summary>
    Android = 2,

    /// <summary>
    /// C++ 项目 (CMakeLists.txt / Makefile / .vcxproj)
    /// </summary>
    Cpp = 3,

    /// <summary>
    /// 通用工具 (.exe 或其他可执行文件)
    /// </summary>
    Tool = 4,

    /// <summary>
    /// 通用文档 (.xlsx/.docx/.pdf 等)
    /// </summary>
    Document = 5,

    /// <summary>
    /// 文件夹
    /// </summary>
    Folder = 6,
    /// <summary>
    /// WebSite
    /// </summary>
    WebSite = 7,
}

/// <summary>
/// 项目启动类型枚举
/// </summary>
public enum LaunchType
{
    /// <summary>
    /// 打开文件（使用系统默认程序）
    /// </summary>
    OpenFile = 0,

    /// <summary>
    /// 打开 exe（使用自定义程序）
    /// </summary>
    OpenExe = 1,

    /// <summary>
    /// 打开网页（使用浏览器）
    /// </summary>
    OpenWebUrl = 2
}
