using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectHub.Core.DependencyInjection;
using ProjectHub.PlatformAbstractions;
using ProjectHub.UI.DependencyInjection;
using ProjectHub.UI.ViewModels;

namespace ProjectHub;

/// <summary>
/// Application entry point and composition root.
/// 
/// Responsibilities:
/// - Host builder configuration (DI, Logging, Configuration)
/// - Service registration for all application layers
/// - Application lifecycle management (Startup, Exit)
/// 
/// DDD Architecture:
/// This class serves as the Composition Root where all dependencies are assembled.
/// Following the Dependency Inversion Principle, concrete implementations are resolved here.
/// 
/// Cross-Platform Design:
/// This class is platform-agnostic. Platform-specific implementations (WPF, Avalonia)
/// are handled by IPlatformService interface implementations.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private readonly ILogger<App> _logger;
    private readonly IPlatformService _platformService;

    /// <summary>
    /// Gets the service provider for resolving services throughout the application.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    public App()
    {
        // Build the host with all configurations
        _host = CreateHostBuilder();

        // Resolve logger and platform service after host is built
        _logger = Services.GetRequiredService<ILogger<App>>();
        _platformService = Services.GetRequiredService<IPlatformService>();
    }

    /// <summary>
    /// Creates and configures the application host.
    /// This method is designed for extensibility - each configuration step can be easily modified.
    /// </summary>
    private IHost CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureApplicationConfiguration)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    /// <summary>
    /// Configures application settings from multiple sources.
    /// Supports JSON files, environment variables, command-line arguments, and in-memory configurations.
    /// </summary>
    private void ConfigureApplicationConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        // Clear default configuration sources
        config.Sources.Clear();

        // Add configuration providers in order of precedence (later overrides earlier)

        // 1. JSON configuration files
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
            optional: true,
            reloadOnChange: true);

        // 2. User-specific settings (optional)
        config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        // 3. Environment variables
        config.AddEnvironmentVariables(prefix: "PROJECTHUB_");

        // 4. Command-line arguments
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 0)
        {
            config.AddCommandLine(args);
        }

        // 5. In-memory configuration (highest precedence)
        config.AddInMemoryCollection(CreateDefaultInMemoryConfiguration());
    }

    /// <summary>
    /// Creates default in-memory configuration values.
    /// These serve as fallback defaults when other configuration sources don't provide values.
    /// </summary>
    private static Dictionary<string, string?> CreateDefaultInMemoryConfiguration()
    {
        return new Dictionary<string, string?>
        {
            // Database configuration
            ["ConnectionStrings:DefaultConnection"] = "Data Source=projecthub.db",

            // Application settings
            ["AppSettings:EnableTelemetry"] = "false",
            ["AppSettings:Theme"] = "Light",
            ["AppSettings:Language"] = "en-US",

            // Feature flags
            ["FeatureFlags:EnableProjectBundles"] = "true",
            ["FeatureFlags:EnableGarbageCollection"] = "true",
            ["FeatureFlags:EnableAutoSave"] = "true"
        };
    }

    /// <summary>
    /// Configures logging providers and minimum log levels.
    /// Easy to extend with additional logging providers (Serilog, NLog, etc.).
    /// </summary>
    private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
    {
        logging.ClearProviders();

        // Built-in logging providers
        logging.AddConsole();
        logging.AddDebug();

        // Optional: Add file logging (uncomment when needed)
        // logging.AddFile("Logs/app.log", new FileLoggerOptions
        // {
        //     MinLevel = LogLevel.Information,
        //     FileSizeLimitBytes = 10_000_000, // 10MB
        //     MaxRollingFiles = 5
        // });

        // Set minimum log level based on configuration
        var minLevel = context.Configuration.GetValue<LogLevel>("Logging:MinLevel", LogLevel.Information);
        logging.SetMinimumLevel(minLevel);

        // Add logging filters for specific namespaces
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    }

    /// <summary>
    /// Registers all application services with the DI container.
    /// Organized by architectural layers for clarity and maintainability.
    /// </summary>
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;

        // ========== Register Views ==========
        RegisterViews(services);

        // ========== Register UI Services (ViewModels) ==========
        services.AddUIServices();

        // ========== Register Infrastructure Services ==========
        services.AddInfrastructureServices(configuration);

        // ========== Register Platform Services ==========
        services.AddSingleton<IPlatformService, WpfPlatformService>();
    }

    /// <summary>
    /// Registers all Views with dependency injection support.
    /// </summary>
    private static void RegisterViews(IServiceCollection services)
    {
        // MainWindow with injected ViewModel
        services.AddSingleton<MainWindow>(provider =>
        {
            var viewModel = provider.GetRequiredService<MainViewModel>();
            return new MainWindow(viewModel);
        });

        // Other windows/dialogs - register as needed
        // services.AddTransient<SettingsWindow>(provider =>
        // {
        //     var viewModel = provider.GetRequiredService<ViewModels.MainViewModel>();
        //     return new SettingsWindow(viewModel);
        // });
    }

    /// <summary>
    /// Application startup event handler.
    /// Initializes the application and displays the main window.
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            _logger.LogInformation("Application starting up...");
            _logger.LogDebug("Command line arguments: {Args}", string.Join(" ", e.Args));

            // Start the host (initializes all services)
            await _host.StartAsync();

            _logger.LogInformation("Host started successfully");
            _logger.LogInformation("Environment: {Environment}",
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

            // Display main window using platform-agnostic service
            await _platformService.ShowMainWindowAsync();

            _logger.LogInformation("Application started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Application failed to start");
            await _platformService.ShowStartupErrorAsync(ex);
            throw;
        }

        base.OnStartup(e);
    }

    /// <summary>
    /// Global exception handler for unhandled UI thread exceptions.
    /// Delegates to platform-specific implementation.
    /// </summary>
    protected void OnDispatcherUnhandledException(
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        // Delegate to platform service for handling
        var shouldContinue = _platformService.ShowErrorDialogAsync(e.Exception).Result;

        // Mark exception as handled to prevent crash if user chose to continue
        e.Handled = shouldContinue;
    }

    /// <summary>
    /// Application exit event handler.
    /// Performs graceful shutdown and resource cleanup.
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger.LogInformation("Application shutting down...");

            // Gracefully stop the host and dispose all services
            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await _host.StopAsync(timeout.Token);
            }

            _logger.LogInformation("Host stopped successfully");
            _logger.LogInformation("Application exited gracefully");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Shutdown timed out, forcing exit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shutdown");
        }
        finally
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }

    /// <summary>
    /// Resolves a service from the dependency injection container.
    /// Convenience method for accessing services throughout the application.
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Attempts to resolve a service, returns null if not registered.
    /// </summary>
    public T? TryGetService<T>() where T : notnull
    {
        return Services.GetService<T>();
    }
}