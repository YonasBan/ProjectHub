using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Mapping;
using ProjectHub.Application.Services;
using ProjectHub.Application.Services.LaunchProviders;
using ProjectHub.Application.ViewModels.DialogViewModel;

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
        // 注册 AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ApplicationMappingProfile>();
        });

        // 注册项目应用服务
        services.AddTransient<IProjectAppService, ProjectAppService>();

        // 注册启动提供者（工厂模式）
        services.AddTransient<ILaunchProvider, OpenFileLaunchProvider>();
        services.AddTransient<ILaunchProvider, OpenExeLaunchProvider>();
        services.AddTransient<ILaunchProvider, OpenWebUrlLaunchProvider>();
        services.AddTransient<ILaunchProvider, OpenCmdLaunchProvider>();
        services.AddTransient<ILaunchProvider, OpenFolderLaunchProvider>();
        services.AddTransient<LaunchProviderFactory>();

        // 注册工作文件夹应用服务
        services.AddTransient<IWorkFolderAppService, WorkFolderAppService>();

        // 注册工作空间应用服务
        services.AddTransient<IWorkSpaceAppService, WorkSpaceAppService>();

        return services;
    }

    /// <summary>
    /// 注册所有 UI 服务（ViewModels）
    /// 这是平台无关的，可以在 WPF 和 Avalonia 之间共享
    /// </summary>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Main ViewModel - Singleton
        services.AddSingleton<ViewModels.MainViewModel>();

        // Sidebar ViewModel - Singleton（与 MainViewModel 生命周期一致）
        services.AddSingleton<ViewModels.SidebarViewModel>();

        // Content ViewModel - Singleton（与 MainViewModel 生命周期一致）
        services.AddSingleton<ViewModels.ContentViewModel>();

        // 对话框 ViewModels - Transient（每次创建新实例）
        services.AddTransient<CreateFolderDialogViewModel>();
        services.AddTransient<ProjectDialogViewModel>();
        services.AddTransient<WorkSpaceDialogViewModel>();
        services.AddTransient<SelectFolderDialogViewModel>();

        return services;
    }
}
