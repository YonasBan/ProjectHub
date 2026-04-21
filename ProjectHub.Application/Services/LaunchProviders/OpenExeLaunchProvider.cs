using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 打开 EXE 启动提供者
/// 使用指定的可执行程序启动
/// </summary>
public class OpenExeLaunchProvider : ILaunchProvider
{
    private readonly IProcessLauncherService _processLauncher;

    public OpenExeLaunchProvider(IProcessLauncherService processLauncher)
    {
        _processLauncher = processLauncher;
    }

    public LaunchType Type => LaunchType.OpenExe;

    public Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(project.DefaultProgram))
        {
            throw new InvalidOperationException("打开 EXE 模式需要指定程序路径");
        }
            
        return _processLauncher.LaunchWithProgramAsync(project.DefaultProgram, project.LaunchArguments, project.RunAsAdmin);
    }
}
