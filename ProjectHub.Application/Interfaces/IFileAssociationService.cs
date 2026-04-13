namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 文件关联服务接口 - 跨平台获取文件默认打开程序
/// </summary>
public interface IFileAssociationService
{
    /// <summary>
    /// 获取指定文件的默认打开程序路径
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>默认程序路径，未找到时返回 null</returns>
    string? GetDefaultProgram(string filePath);

    /// <summary>
    /// 获取指定扩展名的默认打开程序路径
    /// </summary>
    /// <param name="extension">文件扩展名（如 ".txt"）</param>
    /// <returns>默认程序路径，未找到时返回 null</returns>
    string? GetDefaultProgramByExtension(string extension);

    /// <summary>
    /// 获取文件的图标路径或标识
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>图标路径或标识，未找到时返回 null</returns>
    string? GetFileIcon(string filePath);
}
