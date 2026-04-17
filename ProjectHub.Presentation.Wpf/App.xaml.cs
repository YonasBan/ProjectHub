using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectHub.Application.DependencyInjection;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.Services;
using ProjectHub.Application.ViewModels;
using ProjectHub.Application.ViewModels.DialogViewModel;
using ProjectHub.Infrastructure.DependencyInjection;
using ProjectHub.Infrastructure.Persistence;
using ProjectHub.Presentation.Wpf.Dialogs;
using ProjectHub.Presentation.Wpf.Services;
using ReactiveUI;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ProjectHub.Presentation.Wpf;

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

    /// <summary>
    /// Gets the service provider for resolving services throughout the application.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    public App()
    {
        _host = CreateHostBuilder();
        // ✅ 建完 Host 之后桥接
        _host.Services.UseMicrosoftDependencyResolver();
        _logger = Services.GetRequiredService<ILogger<App>>();
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    /// <summary>
    /// Creates and configures the application host.
    /// This method is designed for extensibility - each configuration step can be easily modified.
    /// </summary>
    private IHost CreateHostBuilder()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureApplicationConfiguration)
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(ConfigureServices)
            .Build();
        return host;
    }
    /// <summary>
    /// Registers all Views with dependency injection support.
    /// </summary>
    private static void RegisterViews(IServiceCollection services)
    {
        // MainWindow with injected ViewModel - Singleton to match MainViewModel's lifetime
        services.AddSingleton<MainWindow>(provider =>
        {
            var viewModel = provider.GetRequiredService<MainViewModel>();
            return new MainWindow(viewModel);
        });
    }
    /// <summary>
    /// Registers all application services with the DI container.
    /// Organized by architectural layers for clarity and maintainability.
    /// </summary>
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // ✅ 在 ConfigureServices 最顶部做这三步
        services.UseMicrosoftDependencyResolver();
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();

        // ✅ 然后用 RxAppBuilder 注册平台服务
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            //.WithViewsFromAssembly(typeof(App).Assembly)
            .BuildApp();
        var configuration = context.Configuration;

        // ========== Register Platform-Specific Services (WPF) ==========
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, WpfThemeService>();
        services.AddSingleton<IFileAssociationService, WindowsFileAssociationService>();
        services.AddSingleton<IProcessLauncherService, WindowsProcessLauncherService>();
        // 或者
        var scheduler = new DispatcherScheduler(System.Windows.Application.Current.Dispatcher);
        // ========== Register WPF Scheduler (must be before other registrations) ==========
        services.AddSingleton<IScheduler>(scheduler);
        // ========== Register Localization Service ==========
        RegisterLocalizationServices(services);

        // ========== Register Views ==========
        RegisterViews(services);
        RegisterDialogs(services);
        // ========== Register UI Services (ViewModels) ==========
        services.AddUIServices();

        // ========== Register Application Services ==========
        services.AddApplicationServices();

        // ========== Register Infrastructure Services ==========
        services.AddInfrastructureServices(configuration);
    }

    private void RegisterDialogs(IServiceCollection services)
    {
        AppLocator.CurrentMutable.Register(() => new InputDialog(), typeof(IViewFor<CreateFolderDialogViewModel>));
        AppLocator.CurrentMutable.Register(() => new ProjectDialog(), typeof(IViewFor<ProjectDialogViewModel>));
        AppLocator.CurrentMutable.Register(() => new WorkSpaceDialog(), typeof(IViewFor<WorkSpaceDialogViewModel>));
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
        // Set minimum log level based on configuration
        var minLevel = context.Configuration.GetValue<LogLevel>("Logging:MinLevel", LogLevel.Information);
        logging.SetMinimumLevel(minLevel);

        // Add logging filters for specific namespaces
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
    }

    /// <summary>
    /// Registers localization services.
    /// </summary>
    private static void RegisterLocalizationServices(IServiceCollection services)
    {
        // 注册本地化字符串包装类 - 直接访问资源文件
        services.AddSingleton<LocalizedStrings>();
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

            // 直接使用 Services 而不是创建新的 Scope，避免 Scoped 服务被提前 dispose
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow?.Show();

            _logger.LogInformation("Application started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Application failed to start");
            var idialogServer = Services.GetRequiredService<IDialogService>();
            await idialogServer.ShowMessageAsync("Error", "Application failed to start", "OK");
            throw;
        }

        base.OnStartup(e);
    }

    /// <summary>
    /// Global exception handler for unhandled UI thread exceptions.
    /// Delegates to platform-specific implementation.
    /// </summary>
    private async void App_DispatcherUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.LogCritical(e.Exception, "Unhandled exception occurred");

        try
        {
            var dialogService = Services.GetService<IDialogService>();
            if (dialogService != null)
            {
                await dialogService.ShowMessageAsync("Error", $"发生错误: {e.Exception.Message}", "OK");
            }
        }
        catch
        {
            // Ignore dialog errors
        }

        // Mark exception as handled to prevent crash
        e.Handled = true;
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