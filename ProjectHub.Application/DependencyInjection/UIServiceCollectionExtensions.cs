using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        // Main ViewModel - Singleton (shared across application lifetime)
        services.AddSingleton<ViewModels.MainViewModel>();
        
        // Other ViewModels - register as needed
        // services.AddTransient<ProjectDetailViewModel>();
        // services.AddTransient<SettingsViewModel>();
    }
}
