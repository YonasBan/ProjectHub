using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Interfaces;

/// <summary>
/// 项目启动提供者接口
/// 每种启动类型实现此接口，支持跨平台扩展
/// </summary>
public interface ILaunchProvider
{
    /// <summary>
    /// 支持的启动类型
    /// </summary>
    LaunchType Type { get; }

    /// <summary>
    /// 启动项目
    /// </summary>
    /// <param name="project">项目实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task LaunchAsync(Project project, CancellationToken cancellationToken = default);
}
