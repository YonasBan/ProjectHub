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

    public Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project.WebUrl))
        {
            throw new InvalidOperationException("打开网页模式需要指定网页链接");
        }

        return _processLauncher.LaunchWebUrlAsync(project.WebUrl);
    }
}
