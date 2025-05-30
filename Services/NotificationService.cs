using Microsoft.Extensions.Logging;
using System.Windows;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly List<Notification> _notifications = new();
    private int _nextId = 1;

    public event EventHandler<Notification>? NotificationReceived;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Notification Display Operations

    public async Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // In a real application, you would show a toast notification or snackbar
                // For now, we'll just log the notification
                _logger.LogInformation("Notification: {Title} - {Message} (Type: {Type})", title, message, type);
            });
        });
    }

    public async Task ShowSuccessAsync(string message, string? title = null)
    {
        await ShowNotificationAsync(title ?? "Success", message, NotificationType.Success);
    }

    public async Task ShowWarningAsync(string message, string? title = null)
    {
        await ShowNotificationAsync(title ?? "Warning", message, NotificationType.Warning);
    }

    public async Task ShowErrorAsync(string message, string? title = null)
    {
        await ShowNotificationAsync(title ?? "Error", message, NotificationType.Error);
    }

    public async Task ShowInfoAsync(string message, string? title = null)
    {
        await ShowNotificationAsync(title ?? "Information", message, NotificationType.Info);
    }

    #endregion

    #region User Notification Management

    public async Task<Notification> CreateUserNotificationAsync(string userId, string title, string message,
                                                               NotificationType type = NotificationType.Info,
                                                               NotificationPriority priority = NotificationPriority.Normal,
                                                               bool isActionRequired = false, string? actionUrl = null)
    {
        await Task.Delay(10);

        var notification = new Notification
        {
            Id = _nextId++,
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            IsActionRequired = isActionRequired,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        _notifications.Add(notification);
        NotificationReceived?.Invoke(this, notification);

        _logger.LogInformation("Created notification for user {UserId}: {Title}", userId, title);
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        await Task.Delay(10);
        var userNotifications = _notifications.Where(n => n.UserId == userId);
        
        if (unreadOnly)
        {
            userNotifications = userNotifications.Where(n => !n.IsRead);
        }

        return userNotifications.OrderByDescending(n => n.CreatedAt);
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
    {
        return await GetUserNotificationsAsync(userId, unreadOnly: true);
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        await Task.Delay(10);
        return _notifications.Count(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
    {
        await Task.Delay(10);
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.ReadAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
    {
        await Task.Delay(10);
        var unreadNotifications = _notifications.Where(n => n.UserId == userId && !n.IsRead);
        var count = 0;
        
        foreach (var notification in unreadNotifications)
        {
            notification.ReadAt = DateTime.UtcNow;
            count++;
        }

        return count > 0;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        await Task.Delay(10);
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            _notifications.Remove(notification);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteAllNotificationsAsync(string userId)
    {
        await Task.Delay(10);
        var userNotifications = _notifications.Where(n => n.UserId == userId).ToList();
        foreach (var notification in userNotifications)
        {
            _notifications.Remove(notification);
        }
        return userNotifications.Count > 0;
    }

    #endregion

    #region System-wide Notifications

    public async Task BroadcastNotificationAsync(string title, string message, NotificationType type = NotificationType.Info,
                                                NotificationPriority priority = NotificationPriority.Normal)
    {
        // In a real application, you would broadcast to all connected users
        await ShowNotificationAsync(title, message, type);
        _logger.LogInformation("Broadcast notification: {Title}", title);
    }

    public async Task NotifyDepartmentAsync(int departmentId, string title, string message, NotificationType type = NotificationType.Info)
    {
        // In a real application, you would get all users in the department and send notifications
        await ShowNotificationAsync(title, $"Department {departmentId}: {message}", type);
        _logger.LogInformation("Department notification sent to department {DepartmentId}: {Title}", departmentId, title);
    }

    public async Task NotifyProjectTeamAsync(int projectId, string title, string message, NotificationType type = NotificationType.Info)
    {
        // In a real application, you would get all project team members and send notifications
        await ShowNotificationAsync(title, $"Project {projectId}: {message}", type);
        _logger.LogInformation("Project team notification sent for project {ProjectId}: {Title}", projectId, title);
    }

    #endregion

    #region Business Event Notifications

    public async Task NotifyEmployeeCreatedAsync(int employeeId)
    {
        await ShowSuccessAsync($"New employee has been added to the system (ID: {employeeId})", "Employee Created");
    }

    public async Task NotifyEmployeeUpdatedAsync(int employeeId)
    {
        await ShowInfoAsync($"Employee information has been updated (ID: {employeeId})", "Employee Updated");
    }

    public async Task NotifyEmployeeDeactivatedAsync(int employeeId)
    {
        await ShowWarningAsync($"Employee has been deactivated (ID: {employeeId})", "Employee Deactivated");
    }

    public async Task NotifyProjectCreatedAsync(int projectId)
    {
        await ShowSuccessAsync($"New project has been created (ID: {projectId})", "Project Created");
    }

    public async Task NotifyProjectStatusChangedAsync(int projectId, string oldStatus, string newStatus)
    {
        await ShowInfoAsync($"Project {projectId} status changed from {oldStatus} to {newStatus}", "Project Status Updated");
    }

    public async Task NotifyProjectOverdueAsync(int projectId)
    {
        await ShowWarningAsync($"Project {projectId} is overdue", "Project Overdue");
    }

    public async Task NotifyMilestoneCompletedAsync(int milestoneId)
    {
        await ShowSuccessAsync($"Milestone {milestoneId} has been completed", "Milestone Completed");
    }

    public async Task NotifyMilestoneOverdueAsync(int milestoneId)
    {
        await ShowWarningAsync($"Milestone {milestoneId} is overdue", "Milestone Overdue");
    }

    public async Task NotifyBudgetExceededAsync(int projectId, decimal budgetAmount, decimal actualAmount)
    {
        await ShowErrorAsync($"Project {projectId} budget exceeded: Budget {budgetAmount:C}, Actual {actualAmount:C}", "Budget Exceeded");
    }

    #endregion

    #region Real-time Notifications

    public async Task StartNotificationServiceAsync()
    {
        await Task.Delay(10);
        _logger.LogInformation("Notification service started");
    }

    public async Task StopNotificationServiceAsync()
    {
        await Task.Delay(10);
        _logger.LogInformation("Notification service stopped");
    }

    public async Task<bool> IsConnectedAsync()
    {
        await Task.Delay(10);
        return true; // Always connected in this simple implementation
    }

    #endregion

    #region Notification Templates

    public async Task<string> GenerateWelcomeMessageAsync(string employeeName)
    {
        await Task.Delay(10);
        return $"Welcome to the company, {employeeName}! We're excited to have you on our team.";
    }

    public async Task<string> GenerateProjectAssignmentMessageAsync(string projectName, string role)
    {
        await Task.Delay(10);
        return $"You have been assigned to project '{projectName}' as {role}.";
    }

    public async Task<string> GeneratePasswordResetMessageAsync(string resetUrl)
    {
        await Task.Delay(10);
        return $"Click the following link to reset your password: {resetUrl}";
    }

    public async Task<string> GenerateSystemMaintenanceMessageAsync(DateTime maintenanceTime)
    {
        await Task.Delay(10);
        return $"System maintenance is scheduled for {maintenanceTime:yyyy-MM-dd HH:mm}. Please save your work.";
    }

    #endregion

    #region Settings and Preferences

    public async Task<bool> UpdateNotificationPreferencesAsync(string userId, Dictionary<string, bool> preferences)
    {
        await Task.Delay(10);
        // In a real application, you would store these preferences in a database
        _logger.LogInformation("Updated notification preferences for user {UserId}", userId);
        return true;
    }

    public async Task<Dictionary<string, bool>> GetNotificationPreferencesAsync(string userId)
    {
        await Task.Delay(10);
        // Return default preferences for this simple implementation
        return new Dictionary<string, bool>
        {
            ["Email"] = true,
            ["InApp"] = true,
            ["Desktop"] = false,
            ["Mobile"] = true
        };
    }

    public async Task<bool> IsNotificationEnabledAsync(string userId, string notificationType)
    {
        var preferences = await GetNotificationPreferencesAsync(userId);
        return preferences.TryGetValue(notificationType, out var enabled) && enabled;
    }

    #endregion

    #region Cleanup and Maintenance

    public async Task<int> CleanupOldNotificationsAsync(int retentionDays = 30)
    {
        await Task.Delay(10);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var oldNotifications = _notifications.Where(n => n.CreatedAt < cutoffDate).ToList();
        
        foreach (var notification in oldNotifications)
        {
            _notifications.Remove(notification);
        }

        _logger.LogInformation("Cleaned up {Count} old notifications", oldNotifications.Count);
        return oldNotifications.Count;
    }

    public async Task<long> GetNotificationStorageSizeAsync()
    {
        await Task.Delay(10);
        // Simple calculation based on notification count
        return _notifications.Count * 1024; // Assume 1KB per notification
    }

    #endregion
} 