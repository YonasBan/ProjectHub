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
