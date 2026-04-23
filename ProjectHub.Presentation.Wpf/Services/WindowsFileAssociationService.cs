using Microsoft.Win32;
using ProjectHub.Application.Interfaces;
using System.Diagnostics;
using System.IO;

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

        // 使用 FileAssociationHelper 获取所有打开方式，返回第一个可用的
        var apps = FileAssociationHelper.GetOpenWithApps(extension);
        return apps.FirstOrDefault()?.ExePath;
    }

    public IReadOnlyList<AssociatedProgram> GetAssociatedPrograms(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return Array.Empty<AssociatedProgram>();

        var apps = FileAssociationHelper.GetOpenWithApps(extension);
        return apps.Select(a => new AssociatedProgram(a.ExePath, a.FriendlyName)).ToList();
    }

    public string? GetFileIcon(string filePath)
    {
        // 在 Windows 上，直接返回文件路径，WPF 会通过 Icon 提取器获取图标
        return filePath;
    }
}

/// <summary>
/// 文件关联帮助类 - 从注册表获取文件的打开方式
/// </summary>
public class FileAssociationHelper
{
    /// <summary>
    /// 获取指定扩展名的所有打开方式
    /// </summary>
    public static List<OpenWithApp> GetOpenWithApps(string extension)
    {
        // 确保带点，如 ".pdf"
        if (!extension.StartsWith("."))
            extension = "." + extension;

        var result = new List<OpenWithApp>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ── 途径1：OpenWithProgids（主要来源）──────────────────────
        CollectFromOpenWithProgids(extension, result, seen);

        // ── 途径2：OpenWithList（旧格式兼容）─────────────────────
        CollectFromOpenWithList(extension, result, seen);

        // ── 途径3：默认关联 ProgID ────────────────────────────────
        CollectFromDefaultProgId(extension, result, seen);

        return result;
    }

    // ── 途径1 ──────────────────────────────────────────────────────
    private static void CollectFromOpenWithProgids(
        string extension, List<OpenWithApp> result, HashSet<string> seen)
    {
        using var key = Registry.ClassesRoot
            .OpenSubKey($@"{extension}\OpenWithProgids");
        if (key == null) return;

        foreach (var progId in key.GetValueNames())
        {
            if (string.IsNullOrEmpty(progId)) continue;
            TryAddFromProgId(progId, result, seen);
        }
    }

    // ── 途径2 ──────────────────────────────────────────────────────
    private static void CollectFromOpenWithList(
        string extension, List<OpenWithApp> result, HashSet<string> seen)
    {
        using var key = Registry.ClassesRoot
            .OpenSubKey($@"{extension}\OpenWithList");
        if (key == null) return;

        foreach (var exeName in key.GetSubKeyNames())
        {
            TryAddFromApplicationsKey(exeName, result, seen);
        }
    }

    // ── 途径3 ──────────────────────────────────────────────────────
    private static void CollectFromDefaultProgId(
        string extension, List<OpenWithApp> result, HashSet<string> seen)
    {
        using var extKey = Registry.ClassesRoot.OpenSubKey(extension);
        var progId = extKey?.GetValue("")?.ToString();
        if (!string.IsNullOrEmpty(progId))
            TryAddFromProgId(progId, result, seen);
    }

    // ── 解析 ProgID → 可执行文件 ───────────────────────────────────
    private static void TryAddFromProgId(
        string progId, List<OpenWithApp> result, HashSet<string> seen)
    {
        using var cmdKey = Registry.ClassesRoot
            .OpenSubKey($@"{progId}\shell\open\command");

        var cmd = cmdKey?.GetValue("")?.ToString();
        if (string.IsNullOrEmpty(cmd)) return;

        var exePath = ParseExePath(cmd);
        if (exePath == null || !seen.Add(exePath)) return;

        result.Add(new OpenWithApp
        {
            ExePath = exePath,
            FriendlyName = GetFriendlyName(progId, exePath),
            Command = cmd,
            Source = progId
        });
    }

    // ── 解析 Applications\xxx.exe ──────────────────────────────────
    private static void TryAddFromApplicationsKey(
        string exeName, List<OpenWithApp> result, HashSet<string> seen)
    {
        using var cmdKey = Registry.ClassesRoot
            .OpenSubKey($@"Applications\{exeName}\shell\open\command");

        var cmd = cmdKey?.GetValue("")?.ToString();
        if (string.IsNullOrEmpty(cmd)) return;

        var exePath = ParseExePath(cmd);
        if (exePath == null || !seen.Add(exePath)) return;

        result.Add(new OpenWithApp
        {
            ExePath = exePath,
            FriendlyName = Path.GetFileNameWithoutExtension(exeName),
            Command = cmd,
            Source = exeName
        });
    }

    // ── 从命令行字符串解析出 exe 路径 ──────────────────────────────
    // 格式可能是：
    //   "C:\Program Files\Adobe\...\Acrobat.exe" /A "%1"
    //   C:\Windows\system32\notepad.exe %1
    private static string? ParseExePath(string cmd)
    {
        cmd = cmd.Trim();
        string path;

        if (cmd.StartsWith("\""))
        {
            var end = cmd.IndexOf('"', 1);
            path = end > 1 ? cmd[1..end] : cmd.Trim('"');
        }
        else
        {
            path = cmd.Split(' ')[0];
        }

        // 处理环境变量
        if (path.Contains('%'))
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }

        return File.Exists(path) ? path : null;
    }

    // ── 获取友好名称 ───────────────────────────────────────────────
    private static string GetFriendlyName(string progId, string exePath)
    {
        // 优先从注册表读取 FriendlyAppName
        using var appKey = Registry.ClassesRoot
            .OpenSubKey($@"{progId}\shell\open");
        var name = appKey?.GetValue("FriendlyAppName")?.ToString();
        if (!string.IsNullOrEmpty(name)) return name;

        // 其次读取 exe 的 FileDescription
        try
        {
            var info = FileVersionInfo.GetVersionInfo(exePath);
            if (!string.IsNullOrEmpty(info.FileDescription))
                return info.FileDescription;
        }
        catch { }

        return Path.GetFileNameWithoutExtension(exePath);
    }
}

/// <summary>
/// 打开方式应用信息
/// </summary>
public class OpenWithApp
{
    /// <summary>
    /// 可执行文件路径
    /// </summary>
    public string ExePath { get; set; } = string.Empty;

    /// <summary>
    /// 友好名称
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// 命令行
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// 来源
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
