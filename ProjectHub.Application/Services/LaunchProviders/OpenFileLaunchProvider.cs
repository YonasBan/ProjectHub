using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 打开文件启动提供者
/// 支持使用系统默认程序或自定义程序打开文件
/// </summary>
public class OpenFileLaunchProvider : ILaunchProvider
{
    private readonly IProcessLauncherService _processLauncher;

    public OpenFileLaunchProvider(IProcessLauncherService processLauncher)
    {
        _processLauncher = processLauncher;
    }

    public LaunchType Type => LaunchType.OpenFile;

    public Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        var path = project.Path;
        
        if (!_processLauncher.FileExists(path))
        {
            throw new FileNotFoundException($"Project path not found: {path}");
        }

        // 如果有自定义程序，使用自定义程序打开
        if (!string.IsNullOrEmpty(project.DefaultProgram))
        {
            var arguments = string.IsNullOrWhiteSpace(project.LaunchArguments)
                ? path
                : $"{project.LaunchArguments} \"{path}\"";
            return _processLauncher.LaunchWithProgramAsync(project.DefaultProgram, arguments);
        }
        
        // 否则使用系统默认程序
        return _processLauncher.LaunchWithDefaultProgramAsync(path);
    }
}
