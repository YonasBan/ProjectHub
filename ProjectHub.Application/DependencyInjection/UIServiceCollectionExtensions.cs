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
        // Main ViewModel - Singleton (because it needs to access IServiceProvider which has longer lifetime)
        services.AddSingleton<ViewModels.MainViewModel>();

        services.AddTransient<ViewModels.CreateFolderDialogViewModel>();
        services.AddTransient<ViewModels.ProjectDialogViewModel>();

    }

    /// <summary>
    /// Registers all application services (platform-agnostic).
    /// Note: Platform-specific services (like DialogService) should be registered in the presentation layer.
    /// </summary>
    public static void AddAppServices(this IServiceCollection services)
    {
        // Application Services
        services.AddTransient<IProjectAppService, ProjectAppService>();
        services.AddTransient<IWorkFolderAppService, WorkFolderAppService>();
        services.AddTransient<IWorkSpaceAppService, WorkSpaceAppService>();
        services.AddTransient<ITagAppService, TagAppService>();
    }
}
