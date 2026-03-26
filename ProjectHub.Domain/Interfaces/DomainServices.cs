using ProjectHub.Domain.Entities;
using ProjectHub.Domain.Events;

namespace ProjectHub.Domain.Interfaces;

/// <summary>
/// 领域服务接口 - 定义核心业务逻辑
/// </summary>

/// <summary>
/// 项目启动服务接口
/// 负责根据项目类型调用对应的 IDE/程序打开项目
/// </summary>
public interface IProjectLauncherService
{
    /// <summary>
    /// 启动单个项目
    /// </summary>
    Task LaunchProjectAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量启动项目 (支持顺序和间隔配置)
    /// </summary>
    /// <param name="projects">要启动的项目列表</param>
    /// <param name="order">启动顺序 (projectId -> order)</param>
    /// <param name="intervalSeconds">间隔时间 (秒)</param>
    Task LaunchProjectsAsync(
        IEnumerable<Project> projects, 
        Dictionary<long, int>? order = null, 
        int intervalSeconds = 0,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 垃圾文件清理服务接口
/// 负责扫描和清理项目产生的编译产物、缓存等
/// </summary>
public interface IGarbageCleanerService
{
    /// <summary>
    /// 扫描项目的垃圾文件
    /// </summary>
    /// <returns>返回可清理的目录列表及大小</returns>
    Task<CleanupScanResult> ScanAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行清理
    /// </summary>
    /// <param name="pathsToDelete">要删除的路径列表</param>
    /// <returns>返回实际释放的空间大小 (字节)</returns>
    Task<long> ExecuteCleanupAsync(IEnumerable<string> pathsToDelete, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取项目占用空间
    /// </summary>
    Task<long> CalculateDiskUsageAsync(Project project, CancellationToken cancellationToken = default);
}

/// <summary>
/// 文件监听服务接口
/// 负责监听项目文件变动
/// </summary>
public interface IFileWatcherService
{
    /// <summary>
    /// 开始监听项目
    /// </summary>
    void StartWatching(Project project);

    /// <summary>
    /// 停止监听项目
    /// </summary>
    void StopWatching(long projectId);

    /// <summary>
    /// 停止所有监听
    /// </summary>
    void StopAll();
}

/// <summary>
/// IDE 路径检测服务接口
/// 自动检测系统中安装的 IDE 路径
/// </summary>
public interface IIdeDetectorService
{
    /// <summary>
    /// 检测 Visual Studio 安装路径
    /// </summary>
    IEnumerable<VsInstallation> DetectVisualStudioInstallations();

    /// <summary>
    /// 检测 Android Studio 安装路径
    /// </summary>
    string? DetectAndroidStudioPath();

    /// <summary>
    /// 检测 CLion 安装路径
    /// </summary>
    string? DetectCLionPath();
}

/// <summary>
/// 系统右键菜单集成服务接口
/// </summary>
public interface IShellIntegrationService
{
    /// <summary>
    /// 注册右键菜单"加入 Project Hub"
    /// </summary>
    void RegisterContextMenu();

    /// <summary>
    /// 移除右键菜单
    /// </summary>
    void UnregisterContextMenu();

    /// <summary>
    /// 检查是否已注册
    /// </summary>
    bool IsRegistered();
}

/// <summary>
/// 智能感知服务接口
/// 当用户打开文件时提示是否加入 Project Hub
/// </summary>
public interface ISmartAddService
{
    /// <summary>
    /// 检查路径是否应提示加入
    /// </summary>
    bool ShouldPromptToAdd(string path);

    /// <summary>
    /// 将路径加入忽略列表
    /// </summary>
    void AddToIgnoreList(string path);

    /// <summary>
    /// 从忽略列表移除
    /// </summary>
    void RemoveFromIgnoreList(string path);
}

/// <summary>
/// 域事件分发器接口
/// 用于发布和订阅域事件
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 发布域事件
    /// </summary>
    Task DispatchAsync(BaseDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

// ========== 辅助数据模型 ==========

/// <summary>
/// 清理扫描结果
/// </summary>
public class CleanupScanResult
{
    /// <summary>
    /// 项目总占用空间 (字节)
    /// </summary>
    public long TotalSpaceBytes { get; set; }

    /// <summary>
    /// 可清理空间 (字节)
    /// </summary>
    public long CleanableSpaceBytes { get; set; }

    /// <summary>
    /// 待清理目录列表
    /// </summary>
    public List<CleanupItemInfo> Items { get; set; } = new();
}

/// <summary>
/// 清理项信息
/// </summary>
public class CleanupItemInfo
{
    /// <summary>
    /// 目录路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 目录名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 占用空间 (字节)
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// 清理预设类别
    /// </summary>
    public CleanupCategory Category { get; set; }

    /// <summary>
    /// 是否因排除规则被跳过
    /// </summary>
    public bool IsSkipped { get; set; }

    /// <summary>
    /// 跳过原因
    /// </summary>
    public string? SkipReason { get; set; }
}

/// <summary>
/// 清理类别
/// </summary>
public enum CleanupCategory
{
    Light,      // 轻度清理
    Standard,   // 标准清理
    Deep,       // 深度清理
    Custom      // 自定义
}

/// <summary>
/// Visual Studio 安装信息
/// </summary>
public class VsInstallation
{
    /// <summary>
    /// VS 版本 (如 "2019", "2022")
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 安装路径
    /// </summary>
    public string InstallationPath { get; set; } = string.Empty;

    /// <summary>
    /// devenv.exe 完整路径
    /// </summary>
    public string ExecutablePath => System.IO.Path.Combine(InstallationPath, "devenv.exe");
}
