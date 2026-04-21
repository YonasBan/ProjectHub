namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 跨平台进程启动服务接口
/// 负责启动外部程序/文件，具体实现由平台层提供
/// </summary>
public interface IProcessLauncherService
{
    /// <summary>
    /// 使用系统默认关联程序启动文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>启动是否成功</returns>
    Task<bool> LaunchWithDefaultProgramAsync(string filePath);

    /// <summary>
    /// 使用指定程序启动文件
    /// </summary>
    /// <param name="program">程序路径</param>
    /// <param name="arguments">启动参数（通常是文件路径）</param>
    /// <returns>启动是否成功</returns>
    Task<bool> LaunchWithProgramAsync(string program, string arguments);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// 使用默认浏览器打开网页链接
    /// </summary>
    /// <param name="url">网页 URL</param>
    /// <returns>启动是否成功</returns>
    Task<bool> LaunchWebUrlAsync(string url);
}
