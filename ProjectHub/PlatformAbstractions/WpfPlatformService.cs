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
/// 1. Create AvaloniaPlatformService implementing IPlatformService and IDialogService
/// 2. Replace this registration in App.xaml.cs
/// 3. No other code changes required
/// </summary>
public class WpfPlatformService : IPlatformService, IDialogService
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

    #region IDialogService Implementation

    /// <summary>
    /// Shows a customizable message box using WPF MessageBox.
    /// </summary>
    public async Task<MessageBoxResultType> ShowMessageAsync(
        string message,
        string? title = null,
        MessageBoxButtonType buttons = MessageBoxButtonType.OK,
        MessageBoxIconType icon = MessageBoxIconType.Information,
        MessageBoxResultType? defaultResult = null)
    {
        // Convert custom enums to WPF equivalents
        var wpfButton = buttons switch
        {
            MessageBoxButtonType.OK => MessageBoxButton.OK,
            MessageBoxButtonType.YesNo => MessageBoxButton.YesNo,
            MessageBoxButtonType.YesNoCancel => MessageBoxButton.YesNoCancel,
            MessageBoxButtonType.OKCancel => MessageBoxButton.OKCancel,
            _ => MessageBoxButton.OK
        };

        var wpfIcon = icon switch
        {
            MessageBoxIconType.Information => MessageBoxImage.Information,
            MessageBoxIconType.Warning => MessageBoxImage.Warning,
            MessageBoxIconType.Error => MessageBoxImage.Error,
            MessageBoxIconType.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        };

        var wpfDefaultResult = defaultResult switch
        {
            MessageBoxResultType.OK => MessageBoxResult.OK,
            MessageBoxResultType.Yes => MessageBoxResult.Yes,
            MessageBoxResultType.No => MessageBoxResult.No,
            MessageBoxResultType.Cancel => MessageBoxResult.Cancel,
            _ => MessageBoxResult.None
        };

        // Show the message box
        var result = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            MessageBox.Show(
                message,
                title ?? "Message",
                wpfButton,
                wpfIcon,
                MessageBoxResult.None,
                wpfDefaultResult
            )
        );

        // Convert WPF result to custom enum
        return result switch
        {
            MessageBoxResult.OK => MessageBoxResultType.OK,
            MessageBoxResult.Yes => MessageBoxResultType.Yes,
            MessageBoxResult.No => MessageBoxResultType.No,
            MessageBoxResult.Cancel => MessageBoxResultType.Cancel,
            _ => MessageBoxResultType.None
        };
    }

    /// <summary>
    /// Shows an information message box using WPF MessageBox.
    /// </summary>
    public Task ShowInfoAsync(string message, string? title = null)
    {
        return ShowMessageAsync(message, title ?? "Information", MessageBoxButtonType.OK, MessageBoxIconType.Information);
    }

    /// <summary>
    /// Shows a warning message box using WPF MessageBox.
    /// </summary>
    public Task ShowWarningAsync(string message, string? title = null)
    {
        return ShowMessageAsync(message, title ?? "Warning", MessageBoxButtonType.OK, MessageBoxIconType.Warning);
    }

    /// <summary>
    /// Shows an error message box using WPF MessageBox.
    /// </summary>
    public Task ShowErrorAsync(string message, string? title = null)
    {
        return ShowMessageAsync(message, title ?? "Error", MessageBoxButtonType.OK, MessageBoxIconType.Error);
    }

    /// <summary>
    /// Shows a confirmation dialog using WPF MessageBox.
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string message, string? title = null, bool isYesDefault = true)
    {
        var result = await ShowMessageAsync(
            message,
            title ?? "Confirm",
            MessageBoxButtonType.YesNo,
            MessageBoxIconType.Question,
            isYesDefault ? MessageBoxResultType.Yes : MessageBoxResultType.No
        );
        return result == MessageBoxResultType.Yes;
    }

    /// <summary>
    /// Shows an input dialog using WPF native dialog.
    /// </summary>
    public async Task<string?> ShowInputDialogAsync(
        string message,
        string? title = null,
        string? defaultValue = null,
        string? placeholder = null)
    {
        // Simple implementation using Microsoft.VisualBasic.Interaction.InputBox
        var input = await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            Microsoft.VisualBasic.Interaction.InputBox(
                message,
                title ?? "Input",
                defaultValue ?? string.Empty
            )
        );

        return string.IsNullOrEmpty(input) ? null : input;
    }

    #endregion
}
