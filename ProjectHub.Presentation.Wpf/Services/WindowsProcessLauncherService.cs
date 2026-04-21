using ProjectHub.Application.Interfaces;
using System.Diagnostics;
using System.IO;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// Windows 平台进程启动服务实现
/// </summary>
public class WindowsProcessLauncherService : IProcessLauncherService
{
    public Task<bool> LaunchWithDefaultProgramAsync(string filePath, bool runAsAdmin = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = runAsAdmin ? "runas" : "open"
            };

            Process.Start(startInfo);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> LaunchWithProgramAsync(string program, string arguments, bool runAsAdmin = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = runAsAdmin ? "runas" : "open",
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

    public Task<bool> LaunchWebUrlAsync(string url)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = url,
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
}
