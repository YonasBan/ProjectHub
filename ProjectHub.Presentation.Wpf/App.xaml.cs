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
using ProjectHub.Infrastructure.Services;
using ProjectHub.Presentation.Wpf.Dialogs;
using ProjectHub.Presentation.Wpf.Helpers;
using ProjectHub.Presentation.Wpf.Services;
using ReactiveUI;
using ReactiveUI.Builder;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.InteropServices;
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
    private Mutex? _mutex;
    private const string MutexName = "ProjectHub_SingleInstance_Mutex";

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
    /// MainWindow is registered as Singleton to match MainViewModel's lifetime.
    /// </summary>
    private static void RegisterViews(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
    }
    /// <summary>
    /// Registers all application services with the DI container.
    /// Organized by architectural layers for clarity and maintainability.
    /// </summary>
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;

        // ========== Step 1: Initialize ReactiveUI & Splat ==========
        InitializeReactiveUI(services);

        // ========== Step 2: Register Platform-Specific Services (WPF) ==========
        RegisterPlatformServices(services);

        // ========== Step 3: Register Core Infrastructure ==========
        RegisterCoreInfrastructure(services);

        // ========== Step 4: Register Views and Dialogs ==========
        RegisterViews(services);
        RegisterDialogs();

        // ========== Step 5: Register Application Layers ==========
        services.AddUIServices();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);
    }

    /// <summary>
    /// Initializes ReactiveUI framework and Splat dependency resolver.
    /// Must be called before any ReactiveUI components are used.
    /// </summary>
    private static void InitializeReactiveUI(IServiceCollection services)
    {
        // Bridge Microsoft.Extensions.DependencyInjection with Splat
        services.UseMicrosoftDependencyResolver();
        
        // Initialize Splat locator
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();

        // Configure ReactiveUI for WPF platform
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            .BuildApp();
    }

    /// <summary>
    /// Registers all WPF platform-specific service implementations.
    /// These services provide platform-dependent functionality.
    /// </summary>
    private static void RegisterPlatformServices(IServiceCollection services)
    {
        // UI and Interaction Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, WpfThemeService>();
        services.AddSingleton<TrayIconService>();

        // System Integration Services
        services.AddSingleton<IFileAssociationService, WindowsFileAssociationService>();
        services.AddSingleton<IProcessLauncherService, WindowsProcessLauncherService>();
        services.AddSingleton<IFileExplorerService, WindowsFileExplorerService>();
        services.AddSingleton<ProjectHub.Application.Interfaces.IShellContextMenuService, WindowsShellContextMenuService>();

        // Configuration Service
        services.AddSingleton<IAppSettingsService, JsonAppSettingsService>();

        // WPF Dispatcher Scheduler (required for ReactiveUI threading)
        var scheduler = new DispatcherScheduler(System.Windows.Application.Current.Dispatcher);
        services.AddSingleton<IScheduler>(scheduler);
    }

    /// <summary>
    /// Registers core infrastructure services like localization.
    /// </summary>
    private static void RegisterCoreInfrastructure(IServiceCollection services)
    {
        RegisterLocalizationServices(services);
    }

    /// <summary>
    /// Registers all dialog views with ReactiveUI view locator.
    /// Maps ViewModel interfaces to their corresponding WPF views.
    /// </summary>
    private static void RegisterDialogs()
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
    /// Registers localization services for multi-language support.
    /// Provides access to localized strings throughout the application.
    /// </summary>
    private static void RegisterLocalizationServices(IServiceCollection services)
    {
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

            // 检查单实例
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);
            
            if (!createdNew)
            {
                // 已经有实例在运行，发送消息给现有实例并退出
                _logger.LogInformation("检测到已有实例在运行，发送消息后退出");
                SendArgsToRunningInstance(e.Args);
                Shutdown();
                return;
            }

            // Start the host (initializes all services)
            await _host.StartAsync();

            _logger.LogInformation("Host started successfully");
            _logger.LogInformation("Environment: {Environment}",
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

            // 加载用户配置并应用语言和主题
            await ApplyUserSettingsAsync();

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
    /// 加载用户配置并应用语言和主题
    /// </summary>
    private async Task ApplyUserSettingsAsync()
    {
        try
        {
            var settingsService = Services.GetRequiredService<IAppSettingsService>();
            var settings = settingsService.Load();

            // 应用主题
            var themeService = Services.GetRequiredService<IThemeService>();
            themeService.SetTheme(settings.Theme);

            // 应用语言
            var localizedStrings = Services.GetRequiredService<LocalizedStrings>();
            await localizedStrings.SetCultureAsync(new System.Globalization.CultureInfo(settings.Language));

            _logger.LogInformation("应用用户配置 - 语言: {Language}, 主题: {Theme}", settings.Language, settings.Theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用用户配置时发生错误");
        }
    }

    /// <summary>
    /// 发送参数到已运行的实例
    /// </summary>
    private void SendArgsToRunningInstance(string[] args)
    {
        if (args == null || args.Length == 0) return;

        try
        {
            var argsString = string.Join("|", args);
            _logger.LogInformation($"尝试发送参数到运行实例: {argsString}");
            
            // 查找已运行的 ProjectHub 窗口
            var mainWindowHandle = Win32Helper.FindWindow(null, "ProjectHub");
            
            if (mainWindowHandle == IntPtr.Zero)
            {
                _logger.LogWarning("未找到 ProjectHub 窗口句柄");
                return;
            }
            
            _logger.LogInformation($"找到窗口句柄: {mainWindowHandle}");
            
            // 使用 WM_COPYDATA 发送消息
            var copyData = new Win32Helper.COPYDATASTRUCT
            {
                dwData = IntPtr.Zero,
                cbData = (argsString.Length + 1) * 2, // Unicode 每个字符2字节，+1为null终止符
                lpData = Marshal.StringToHGlobalUni(argsString)
            };

            var result = Win32Helper.SendMessage(mainWindowHandle, Win32Helper.WM_COPYDATA, IntPtr.Zero, ref copyData);
            Marshal.FreeHGlobal(copyData.lpData);
            
            _logger.LogInformation($"SendMessage 返回结果: {result}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送参数到运行实例失败");
        }
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