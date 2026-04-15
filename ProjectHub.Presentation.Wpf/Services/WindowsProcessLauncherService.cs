using ProjectHub.Application.Interfaces;
using System.Diagnostics;
using System.IO;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// Windows 平台进程启动服务实现
/// </summary>
public class WindowsProcessLauncherService : IProcessLauncherService
{
    public Task<bool> LaunchWithDefaultProgramAsync(string filePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(startInfo);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> LaunchWithProgramAsync(string program, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = $"\"{arguments}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public bool FileExists(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }
}
