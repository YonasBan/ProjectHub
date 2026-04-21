namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 跨平台文件资源管理器服务接口
/// 负责在系统文件资源管理器中打开文件或文件夹
/// </summary>
public interface IFileExplorerService
{
    /// <summary>
    /// 在文件资源管理器中打开指定路径的文件夹，并选中文件（如果路径是文件）
    /// </summary>
    /// <param name="path">文件或文件夹路径</param>
    /// <returns>操作是否成功</returns>
    Task<bool> OpenFolderAndSelectItemAsync(string path);

    /// <summary>
    /// 在文件资源管理器中打开指定文件夹
    /// </summary>
    /// <param name="folderPath">文件夹路径</param>
    /// <returns>操作是否成功</returns>
    Task<bool> OpenFolderAsync(string folderPath);
}
