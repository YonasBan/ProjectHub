using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using ProjectHub.PlatformAbstractions;

namespace ProjectHub;

/// <summary>
/// WPF-specific implementation of platform services.
/// This class handles all WPF-specific UI operations.
/// 
/// To migrate to Avalonia:
/// 1. Create AvaloniaPlatformService implementing IPlatformService
/// 2. Replace this registration in App.xaml.cs
/// 3. No other code changes required
/// </summary>
public class WpfPlatformService : IPlatformService
{
    private readonly ILogger<WpfPlatformService> _logger;
    private readonly MainWindow _mainWindow;

    public WpfPlatformService(ILogger<WpfPlatformService> logger, MainWindow mainWindow)
    {
        _logger = logger;
        _mainWindow = mainWindow;
    }

    /// <summary>
    /// Shows the main window using WPF API.
    /// </summary>
    public Task ShowMainWindowAsync()
    {
        _mainWindow.Show();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows error dialog with user-friendly message using WPF MessageBox.
    /// </summary>
    public Task<bool> ShowErrorDialogAsync(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception in UI thread");

        var result = MessageBox.Show(
            $"An unexpected error occurred:\n\n{GetUserFriendlyMessage(exception)}\n\n" +
            $"Would you like to continue running?\n\n" +
            $"Click 'Yes' to continue, 'No' to exit.",
            "Application Error",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error
        );

        if (result == MessageBoxResult.No)
        {
            _logger.LogInformation("User chose to exit after error");
            Shutdown(1);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <summary>
    /// Shows startup error using WPF MessageBox.
    /// </summary>
    public Task ShowStartupErrorAsync(Exception exception)
    {
        MessageBox.Show(
            $"Failed to start application:\n\n{exception.Message}\n\n" +
            $"Please check the logs for more details.",
            "Startup Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Shuts down WPF application.
    /// </summary>
    public void Shutdown(int exitCode = 0)
    {
        System.Windows.Application.Current.Shutdown(exitCode);
    }

    /// <summary>
    /// Converts technical exception messages to user-friendly text.
    /// </summary>
    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "Access denied. Please check your permissions.",
            System.IO.FileNotFoundException => "Required file not found. Please reinstall the application.",
            TimeoutException => "Operation timed out. Please try again.",
            ArgumentException => "Invalid argument provided.",
            _ => exception.Message
        };
    }
}
