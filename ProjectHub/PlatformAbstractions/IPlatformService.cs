using System;
using System.Threading.Tasks;

namespace ProjectHub.PlatformAbstractions;

/// <summary>
/// Platform abstraction for UI framework operations.
/// This interface allows switching between WPF and Avalonia without changing core logic.
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Shows the main window asynchronously.
    /// </summary>
    Task ShowMainWindowAsync();

    /// <summary>
    /// Shows a user-friendly error message dialog.
    /// </summary>
    /// <param name="exception">The exception to display</param>
    /// <returns>True if user chose to continue, false if chose to exit</returns>
    Task<bool> ShowErrorDialogAsync(Exception exception);

    /// <summary>
    /// Shows a startup error message.
    /// </summary>
    /// <param name="exception">The startup exception</param>
    Task ShowStartupErrorAsync(Exception exception);

    /// <summary>
    /// Shuts down the application with specified exit code.
    /// </summary>
    /// <param name="exitCode">Exit code (0 = success, 1 = error)</param>
    void Shutdown(int exitCode = 0);
}

/// <summary>
/// Provides platform-agnostic dialog services for ViewModels.
/// This abstraction allows showing dialogs without coupling to specific UI frameworks.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an information message box.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">Optional title (defaults to "Information")</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ShowInfoAsync(string message, string? title = null);

    /// <summary>
    /// Shows a warning message box.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">Optional title (defaults to "Warning")</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ShowWarningAsync(string message, string? title = null);

    /// <summary>
    /// Shows an error message box.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">Optional title (defaults to "Error")</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ShowErrorAsync(string message, string? title = null);

    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">Optional title (defaults to "Confirm")</param>
    /// <param name="isYesDefault">If true, Yes button is default (press Enter to confirm)</param>
    /// <returns>True if user confirmed (clicked Yes), false otherwise</returns>
    Task<bool> ShowConfirmationAsync(string message, string? title = null, bool isYesDefault = true);

    /// <summary>
    /// Shows an input dialog to get text from user.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="title">Optional title (defaults to "Input")</param>
    /// <param name="defaultValue">Optional default value</param>
    /// <param name="placeholder">Optional placeholder text</param>
    /// <returns>The text entered by user, or null if cancelled</returns>
    Task<string?> ShowInputDialogAsync(
        string message,
        string? title = null,
        string? defaultValue = null,
        string? placeholder = null);
}
