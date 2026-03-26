namespace ProjectHub.Core.Extensions;

/// <summary>
/// 文件扩展方法
/// </summary>
public static class FileExtensions
{
    /// <summary>
    /// 格式化文件大小为人类可读格式
    /// </summary>
    public static string FormatFileSize(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 安全删除目录 (带重试机制)
    /// </summary>
    public static void DeleteDirectoryWithRetry(this DirectoryInfo directoryInfo, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                    return;
                }
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                // 文件可能被占用，等待后重试
                Thread.Sleep(100 * (i + 1));
            }
        }
    }

    /// <summary>
    /// 获取目录大小 (递归计算所有文件)
    /// </summary>
    public static long GetDirectorySize(this DirectoryInfo directoryInfo)
    {
        if (!directoryInfo.Exists)
            return 0;

        try
        {
            return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => f.Length);
        }
        catch (UnauthorizedAccessException)
        {
            // 没有权限访问某些子目录
            return 0;
        }
    }
}

/// <summary>
/// TimeSpan 扩展方法
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    /// 格式化时长为人类可读格式
    /// </summary>
    public static string ToHumanReadableString(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMinutes < 1)
            return $"{timeSpan.Seconds}秒";
        
        if (timeSpan.TotalHours < 1)
            return $"{(int)timeSpan.TotalMinutes}分钟";
        
        if (timeSpan.TotalDays < 1)
            return $"{(int)timeSpan.Hours}小时 {timeSpan.Minutes}分钟";
        
        return $"{(int)timeSpan.TotalDays}天 {(int)timeSpan.Hours % 24}小时";
    }
}
