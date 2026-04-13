using Microsoft.Win32;
using ProjectHub.Application.Interfaces;
using System.IO;
using System.Runtime.InteropServices;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// Windows 平台文件关联服务实现
/// </summary>
public class WindowsFileAssociationService : IFileAssociationService
{
    public string? GetDefaultProgram(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return GetDefaultProgramByExtension(extension);
    }

    public string? GetDefaultProgramByExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return null;

        // 确保扩展名以点开头的
        if (!extension.StartsWith('.'))
            extension = "." + extension;

        try
        {
            // 方法1：从注册表读取文件关联
            var progId = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extension}", "", null) as string;
            
            if (!string.IsNullOrEmpty(progId))
            {
                // 读取 ProgId 的 shell\open\command
                var command = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progId}\shell\open\command", "", null) as string;
                if (!string.IsNullOrEmpty(command))
                {
                    return ExtractExecutablePath(command);
                }
            }

            // 方法2：直接读取 SystemFileAssociations
            var sysCommand = Registry.GetValue($@"HKEY_CLASSES_ROOT\SystemFileAssociations\{extension}\shell\open\command", "", null) as string;
            if (!string.IsNullOrEmpty(sysCommand))
            {
                return ExtractExecutablePath(sysCommand);
            }

            // 方法3：使用 ApplicationAssociation 查询（Windows 8+）
            return GetDefaultProgramViaApi(extension);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string? GetFileIcon(string filePath)
    {
        // 在 Windows 上，直接返回文件路径，WPF 会通过 Icon 提取器获取图标
        return filePath;
    }

    /// <summary>
    /// 从命令字符串中提取可执行文件路径
    /// </summary>
    private string? ExtractExecutablePath(string command)
    {
        if (string.IsNullOrEmpty(command))
            return null;

        // 处理带引号的路径 "path\to\exe" %1
        command = command.Trim();
        
        if (command.StartsWith('"'))
        {
            var endQuote = command.IndexOf('"', 1);
            if (endQuote > 0)
            {
                return command.Substring(1, endQuote - 1);
            }
        }

        // 处理不带引号的路径 path\to\exe %1
        var spaceIndex = command.IndexOf(' ');
        if (spaceIndex > 0)
        {
            return command.Substring(0, spaceIndex);
        }

        return command;
    }

    /// <summary>
    /// 使用 Windows API 获取默认程序（Windows 8+）
    /// </summary>
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int AssocQueryString(
        AssocF flags,
        AssocStr str,
        string pszAssoc,
        string? pszExtra,
        [Out] char[] pszOut,
        ref int pcchOut);

    [Flags]
    private enum AssocF : uint
    {
        None = 0,
        OpenByExeName = 0x2,
    }

    private enum AssocStr : uint
    {
        Command = 1,
        Executable = 2,
    }

    private string? GetDefaultProgramViaApi(string extension)
    {
        try
        {
            var buffer = new char[1024];
            var bufferSize = buffer.Length;

            var result = AssocQueryString(
                AssocF.None,
                AssocStr.Executable,
                extension,
                "open",
                buffer,
                ref bufferSize);

            if (result == 0) // S_OK
            {
                return new string(buffer, 0, bufferSize - 1);
            }
        }
        catch
        {
            // API 调用失败，忽略
        }

        return null;
    }
}
