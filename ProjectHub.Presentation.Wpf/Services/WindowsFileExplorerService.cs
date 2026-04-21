using ProjectHub.Application.Interfaces;
using System.Diagnostics;
using System.IO;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// Windows 平台文件资源管理器服务实现
/// </summary>
public class WindowsFileExplorerService : IFileExplorerService
{
    public Task<bool> OpenFolderAndSelectItemAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return Task.FromResult(false);

            // 获取文件的目录路径
            var directoryPath = File.Exists(path) ? Path.GetDirectoryName(path) : path;

            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                return Task.FromResult(false);

            // 使用 explorer.exe 打开文件夹并选中文件
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = File.Exists(path)
                    ? $"/select,\"{path}\""
                    : $"\"{directoryPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> OpenFolderAsync(string folderPath)
    {
        try
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return Task.FromResult(false);

            // 使用 explorer.exe 打开文件夹
            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
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
