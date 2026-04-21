using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 运行 CMD 命令启动提供者
/// 在命令行中执行指定的命令
/// </summary>
public class OpenCmdLaunchProvider : ILaunchProvider
{
    private readonly IProcessLauncherService _processLauncher;

    public OpenCmdLaunchProvider(IProcessLauncherService processLauncher)
    {
        _processLauncher = processLauncher;
    }

    public LaunchType Type => LaunchType.OpenCmd;

    public async Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project.CmdCommand))
        {
            throw new InvalidOperationException("运行 CMD 命令模式需要指定要执行的命令");
        }

        // 使用 cmd.exe /c 执行命令
        await _processLauncher.LaunchWithProgramAsync("cmd.exe", $"/c {project.CmdCommand}");
    }
}
