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
            // 方法1：使用 Windows API 获取默认程序（最可靠，支持 Windows 8+）
            var apiResult = GetDefaultProgramViaApi(extension);
            if (!string.IsNullOrEmpty(apiResult) && File.Exists(apiResult))
            {
                return apiResult;
            }

            // 方法2：从注册表读取文件关联的 ProgId
            var progId = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extension}", "", null) as string;
            
            if (!string.IsNullOrEmpty(progId))
            {
                // 尝试读取 ProgId 的 shell\open\command
                var command = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progId}\shell\open\command", "", null) as string;
                if (!string.IsNullOrEmpty(command))
                {
                    var exePath = ExtractExecutablePath(command);
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        return exePath;
                    }
                }

                // 尝试读取 ProgId 的 shell\open\ddeexec 失败后的 command
                var openWithProgIds = Registry.GetValue($@"HKEY_CLASSES_ROOT\{extension}\OpenWithProgids", "", null) as string;
                if (!string.IsNullOrEmpty(openWithProgIds))
                {
                    var progIds = openWithProgIds.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var pid in progIds)
                    {
                        var cmd = Registry.GetValue($@"HKEY_CLASSES_ROOT\{pid}\shell\open\command", "", null) as string;
                        if (!string.IsNullOrEmpty(cmd))
                        {
                            var exe = ExtractExecutablePath(cmd);
                            if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
                            {
                                return exe;
                            }
                        }
                    }
                }
            }

            // 方法3：直接读取 SystemFileAssociations
            var sysCommand = Registry.GetValue($@"HKEY_CLASSES_ROOT\SystemFileAssociations\{extension}\shell\open\command", "", null) as string;
            if (!string.IsNullOrEmpty(sysCommand))
            {
                var sysExe = ExtractExecutablePath(sysCommand);
                if (!string.IsNullOrEmpty(sysExe) && File.Exists(sysExe))
                {
                    return sysExe;
                }
            }

            // 方法4：从 UserChoice 读取（Windows 10+ 用户自定义的默认程序）
            var userChoicePath = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\UserChoice";
            using (var userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoicePath))
            {
                if (userChoiceKey != null)
                {
                    var progIdValue = userChoiceKey.GetValue("ProgId") as string;
                    if (!string.IsNullOrEmpty(progIdValue))
                    {
                        var userCommand = Registry.GetValue($@"HKEY_CLASSES_ROOT\{progIdValue}\shell\open\command", "", null) as string;
                        if (!string.IsNullOrEmpty(userCommand))
                        {
                            var userExe = ExtractExecutablePath(userCommand);
                            if (!string.IsNullOrEmpty(userExe) && File.Exists(userExe))
                            {
                                return userExe;
                            }
                        }
                    }
                }
            }

            return null;
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
                var quotedPath = command.Substring(1, endQuote - 1);
                // 处理环境变量如 %SystemRoot%
                if (quotedPath.Contains('%'))
                {
                    quotedPath = Environment.ExpandEnvironmentVariables(quotedPath);
                }
                return quotedPath;
            }
        }

        // 处理不带引号的路径 path\to\exe %1
        var spaceIndex = command.IndexOf(' ');
        if (spaceIndex > 0)
        {
            var unquotedPath = command.Substring(0, spaceIndex);
            // 处理环境变量
            if (unquotedPath.Contains('%'))
            {
                unquotedPath = Environment.ExpandEnvironmentVariables(unquotedPath);
            }
            return unquotedPath;
        }

        // 没有空格，整个字符串就是路径
        if (command.Contains('%'))
        {
            command = Environment.ExpandEnvironmentVariables(command);
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
            // 方法A：获取可执行文件路径
            var exePath = AssocQueryString(AssocStr.Executable, extension);
            if (!string.IsNullOrEmpty(exePath))
            {
                // 处理环境变量
                if (exePath.Contains('%'))
                {
                    exePath = Environment.ExpandEnvironmentVariables(exePath);
                }
                
                // 有些情况下返回的是不带路径的exe名称，需要查找完整路径
                if (!Path.IsPathRooted(exePath))
                {
                    exePath = FindExecutableInPath(exePath) ?? exePath;
                }

                if (File.Exists(exePath))
                {
                    return exePath;
                }
            }

            // 方法B：获取命令行并提取路径
            var command = AssocQueryString(AssocStr.Command, extension);
            if (!string.IsNullOrEmpty(command))
            {
                var extractedPath = ExtractExecutablePath(command);
                if (!string.IsNullOrEmpty(extractedPath))
                {
                    if (!Path.IsPathRooted(extractedPath))
                    {
                        extractedPath = FindExecutableInPath(extractedPath) ?? extractedPath;
                    }

                    if (File.Exists(extractedPath))
                    {
                        return extractedPath;
                    }
                }
            }
        }
        catch
        {
            // API 调用失败，忽略
        }

        return null;
    }

    /// <summary>
    /// 在 PATH 环境变量中查找可执行文件
    /// </summary>
    private string? FindExecutableInPath(string exeName)
    {
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            var paths = pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, exeName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                // 尝试添加 .exe 扩展名
                if (!exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    fullPath = Path.Combine(path, exeName + ".exe");
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return null;
    }

    /// <summary>
    /// 使用 AssocQueryString API 查询关联字符串
    /// </summary>
    private string? AssocQueryString(AssocStr assocStr, string extension)
    {
        var buffer = new char[1024];
        var bufferSize = buffer.Length;

        var result = AssocQueryString(
            AssocF.None,
            assocStr,
            extension,
            "open",
            buffer,
            ref bufferSize);

        if (result == 0) // S_OK
        {
            return new string(buffer, 0, bufferSize - 1);
        }

        return null;
    }
}
