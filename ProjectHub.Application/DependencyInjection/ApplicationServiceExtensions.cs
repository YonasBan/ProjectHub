using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Services;

namespace ProjectHub.Application.DependencyInjection;

/// <summary>
/// Application 层依赖注入扩展方法
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// 注册所有应用服务
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册项目应用服务
        services.AddScoped<IProjectAppService, ProjectAppService>();

        // 注册工作文件夹应用服务
        services.AddScoped<IWorkFolderAppService, WorkFolderAppService>();

        // 注册工作空间应用服务
        services.AddScoped<IWorkSpaceAppService, WorkSpaceAppService>();

        // 注册标签应用服务
        services.AddScoped<ITagAppService, TagAppService>();

        // TODO: 注册清理应用服务
        // services.AddScoped<ICleanupAppService, CleanupAppService>();

        return services;
    }
}
