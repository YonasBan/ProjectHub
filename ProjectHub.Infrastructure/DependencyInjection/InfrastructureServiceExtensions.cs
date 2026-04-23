using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Domain.Interfaces;
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Infrastructure.Repositories;

namespace ProjectHub.Infrastructure.DependencyInjection;

/// <summary>
/// Infrastructure 层依赖注入扩展方法
/// 负责注册数据库上下文、仓储等基础设施服务
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// 注册基础设施服务（数据库、仓储等）
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册数据库上下文
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=projecthub.db";
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));

        // 注册仓储
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<IWorkFolderRepository, WorkFolderRepository>();
        services.AddTransient<IWorkSpaceRepository, WorkSpaceRepository>();
        services.AddTransient<IProjectWorkSpaceRepository, ProjectWorkSpaceRepository>();

        return services;
    }
}
