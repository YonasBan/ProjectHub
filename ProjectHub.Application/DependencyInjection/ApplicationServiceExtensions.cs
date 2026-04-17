using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Services;

namespace ProjectHub.Application.DependencyInjection;

/// <summary>
/// Application 层依赖注入扩展方法
/// 包含应用服务和 UI 服务（ViewModels）的注册
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// 注册所有应用服务
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 注册项目应用服务
        services.AddTransient<IProjectAppService, ProjectAppService>();

        // 注册工作文件夹应用服务
        services.AddTransient<IWorkFolderAppService, WorkFolderAppService>();

        // 注册工作空间应用服务
        services.AddTransient<IWorkSpaceAppService, WorkSpaceAppService>();

        // 注册标签应用服务
        services.AddTransient<ITagAppService, TagAppService>();

        // TODO: 注册清理应用服务
        // services.AddScoped<ICleanupAppService, CleanupAppService>();

        return services;
    }

    /// <summary>
    /// 注册所有 UI 服务（ViewModels）
    /// 这是平台无关的，可以在 WPF 和 Avalonia 之间共享
    /// </summary>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Main ViewModel - Singleton（因为它需要访问具有更长生命周期的 IServiceProvider）
        services.AddSingleton<ViewModels.MainViewModel>();

        // 对话框 ViewModels - Transient（每次创建新实例）
        services.AddTransient<ViewModels.CreateFolderDialogViewModel>();
        services.AddTransient<ViewModels.ProjectDialogViewModel>();
        services.AddTransient<ViewModels.WorkSpaceDialogViewModel>();
        services.AddTransient<ViewModels.SelectFolderDialogViewModel>();

        return services;
    }
}
