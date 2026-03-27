using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectHub.Application.Interfaces
{
    public interface INotificationService
    {
        Task ShowToastAsync(string message, NotificationSeverity severity = NotificationSeverity.Info,
            int durationMs = 3000);
    }
    public enum NotificationSeverity { Info, Success, Warning, Error }
}
