using ProjectHub.Domain.Interfaces;

namespace ProjectHub.Application.DTOs;

/// <summary>
/// 清理扫描结果 DTO
/// </summary>
public class CleanupScanResultDto
{
    public long TotalSpaceBytes { get; set; }
    public long CleanableSpaceBytes { get; set; }
    public string TotalSpaceDisplay => FormatSize(TotalSpaceBytes);
    public string CleanableSpaceDisplay => FormatSize(CleanableSpaceBytes);
    public IReadOnlyList<CleanupItemInfoDto> Items { get; set; } = [];

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// 清理项信息 DTO
/// </summary>
public class CleanupItemInfoDto
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeDisplay => $"{FormatSize(SizeBytes)}";
    public CleanupCategory Category { get; set; }
    public bool IsSkipped { get; set; }
    public string? SkipReason { get; set; }

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// 清理预设方案 DTO
/// </summary>
public class CleanupProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CleanupCategory Category { get; set; }
    public IEnumerable<string> IncludedPatterns { get; set; } = [];
}
