using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 打开网页启动提供者
/// 使用浏览器打开网页链接
/// </summary>
public class OpenWebUrlLaunchProvider : ILaunchProvider
{
    private readonly IProcessLauncherService _processLauncher;

    public OpenWebUrlLaunchProvider(IProcessLauncherService processLauncher)
    {
        _processLauncher = processLauncher;
    }

    public LaunchType Type => LaunchType.OpenWebUrl;

    public async Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project.WebUrl))
        {
            throw new InvalidOperationException("打开网页模式需要指定网页链接");
        }

        // 支持多个链接用分号分隔
        var urls = project.WebUrl.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(u => u.Trim())
                                 .Where(u => !string.IsNullOrWhiteSpace(u))
                                 .ToList();

        if (urls.Count == 0)
        {
            throw new InvalidOperationException("没有有效的网页链接");
        }

        // 并行打开所有网页
        var tasks = urls.Select(url => _processLauncher.LaunchWebUrlAsync(url));
        await Task.WhenAll(tasks);
    }
}
