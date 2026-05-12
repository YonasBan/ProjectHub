using ProjectHub.Application.Interfaces;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Services.LaunchProviders;

/// <summary>
/// 打开文件夹启动提供者
/// 在文件资源管理器中打开项目所在文件夹
/// </summary>
public class OpenFolderLaunchProvider : ILaunchProvider
{
    private readonly IFileExplorerService _fileExplorerService;

    public OpenFolderLaunchProvider(IFileExplorerService fileExplorerService)
    {
        _fileExplorerService = fileExplorerService;
    }

    public LaunchType Type => LaunchType.OpenFolder;

    public async Task LaunchAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project.Path))
        {
            throw new InvalidOperationException("打开文件夹模式需要指定文件夹路径");
        }

        var success = await _fileExplorerService.OpenFolderAsync(project.Path);
        
        if (!success)
        {
            throw new InvalidOperationException($"无法打开文件夹: {project.Path}");
        }
    }
}
