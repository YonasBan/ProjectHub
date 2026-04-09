using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Services;

namespace ProjectHub.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering UI services (ViewModels).
/// This is platform-agnostic and can be shared across WPF and Avalonia.
/// </summary>
public static class UIServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ViewModels with appropriate lifecycles.
    /// </summary>
    public static void AddUIServices(this IServiceCollection services)
    {
        // Main ViewModel - Scoped (because it depends on scoped services)
        services.AddScoped<ViewModels.MainViewModel>();

        services.AddTransient<ViewModels.CreateFolderDialogViewModel>();
        services.AddTransient<ViewModels.AddProjectDialogViewModel>();

    }

    /// <summary>
    /// Registers all application services (platform-agnostic).
    /// Note: Platform-specific services (like DialogService) should be registered in the presentation layer.
    /// </summary>
    public static void AddAppServices(this IServiceCollection services)
    {
        // Application Services
        services.AddScoped<IProjectAppService, ProjectAppService>();
        services.AddScoped<IWorkFolderAppService, WorkFolderAppService>();
        services.AddScoped<IWorkSpaceAppService, WorkSpaceAppService>();
        services.AddScoped<ITagAppService, TagAppService>();
    }
}
