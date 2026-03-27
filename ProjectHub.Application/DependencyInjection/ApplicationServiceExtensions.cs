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
        // 注册工作空间应用服务
        services.AddScoped<IWorkSpaceAppService, WorkSpaceAppService>();
        
        // TODO: 注册其他应用服务
        // services.AddScoped<IProjectAppService, ProjectAppService>();
        // services.AddScoped<IWorkFolderAppService, WorkFolderAppService>();
        // services.AddScoped<ITagAppService, TagAppService>();
        // services.AddScoped<ICleanupAppService, CleanupAppService>();
        // services.AddScoped<IProjectBundleAppService, ProjectBundleAppService>();

        return services;
    }
}
