using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories;

namespace ProjectHub.Core.DependencyInjection;

/// <summary>
/// 依赖注入配置类
/// 负责注册所有服务到 DI 容器
/// 
/// DDD 设计要点:
/// - 控制反转：所有依赖通过接口注入
/// - 生命周期管理：Singleton/Scoped/Transient
/// - 可测试性：便于 Mock 和单元测试
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册应用服务
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: 注册应用服务 (实现后取消注释)
        // services.AddScoped<IProjectAppService, ProjectAppService>();
        // services.AddScoped<IGroupAppService, GroupAppService>();
        // services.AddScoped<ITagAppService, TagAppService>();
        // services.AddScoped<ICleanupAppService, CleanupAppService>();
        // services.AddScoped<IProjectBundleAppService, ProjectBundleAppService>();
        
        // 注意：应用服务需要在 Application 层自行注册
        // 调用 services.AddApplicationServicesFromAssembly() 来注册

        return services;
    }

    /// <summary>
    /// 注册领域服务
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // TODO: 注册领域服务 (实现后取消注释)
        // services.AddScoped<IProjectLauncherService, ProjectLauncherService>();
        // services.AddScoped<IGarbageCleanerService, GarbageCleanerService>();
        // services.AddSingleton<IFileWatcherService, FileWatcherService>();
        // services.AddScoped<IIdeDetectorService, IdeDetectorService>();
        // services.AddScoped<IShellIntegrationService, ShellIntegrationService>();
        // services.AddSingleton<ISmartAddService, SmartAddService>();
        // services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    /// <summary>
    /// 注册基础设施服务
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册数据库上下文
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=projecthub.db";
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // 注册仓储
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IWorkFolderRepository, WorkFolderRepository>();
        services.AddScoped<IWorkSpaceRepository, WorkSpaceRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // TODO: 注册其他基础设施服务
        // services.AddScoped<IProjectBundleRepository, ProjectBundleRepository>();

        return services;
    }

    /// <summary>
    /// 注册所有服务 (便捷方法)
    /// </summary>
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddDomainServices();
        services.AddInfrastructureServices(configuration);

        return services;
    }
}
