namespace WpfApp2.Services.Interfaces;

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsRead => ReadAt.HasValue;
    public bool IsActionRequired { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public interface INotificationService
{
    // Notification Display Operations
    Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info);
    Task ShowSuccessAsync(string message, string? title = null);
    Task ShowWarningAsync(string message, string? title = null);
    Task ShowErrorAsync(string message, string? title = null);
    Task ShowInfoAsync(string message, string? title = null);

    // User Notification Management
    Task<Notification> CreateUserNotificationAsync(string userId, string title, string message, 
                                                  NotificationType type = NotificationType.Info,
                                                  NotificationPriority priority = NotificationPriority.Normal,
                                                  bool isActionRequired = false, string? actionUrl = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
    Task<int> GetUnreadNotificationCountAsync(string userId);
    Task<bool> MarkNotificationAsReadAsync(int notificationId);
    Task<bool> MarkAllNotificationsAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int notificationId);
    Task<bool> DeleteAllNotificationsAsync(string userId);

    // System-wide Notifications
    Task BroadcastNotificationAsync(string title, string message, NotificationType type = NotificationType.Info,
                                   NotificationPriority priority = NotificationPriority.Normal);
    Task NotifyDepartmentAsync(int departmentId, string title, string message, NotificationType type = NotificationType.Info);
    Task NotifyProjectTeamAsync(int projectId, string title, string message, NotificationType type = NotificationType.Info);

    // Business Event Notifications
    Task NotifyEmployeeCreatedAsync(int employeeId);
    Task NotifyEmployeeUpdatedAsync(int employeeId);
    Task NotifyEmployeeDeactivatedAsync(int employeeId);
    Task NotifyProjectCreatedAsync(int projectId);
    Task NotifyProjectStatusChangedAsync(int projectId, string oldStatus, string newStatus);
    Task NotifyProjectOverdueAsync(int projectId);
    Task NotifyMilestoneCompletedAsync(int milestoneId);
    Task NotifyMilestoneOverdueAsync(int milestoneId);
    Task NotifyBudgetExceededAsync(int projectId, decimal budgetAmount, decimal actualAmount);

    // Real-time Notifications
    event EventHandler<Notification>? NotificationReceived;
    Task StartNotificationServiceAsync();
    Task StopNotificationServiceAsync();
    Task<bool> IsConnectedAsync();

    // Notification Templates
    Task<string> GenerateWelcomeMessageAsync(string employeeName);
    Task<string> GenerateProjectAssignmentMessageAsync(string projectName, string role);
    Task<string> GeneratePasswordResetMessageAsync(string resetUrl);
    Task<string> GenerateSystemMaintenanceMessageAsync(DateTime maintenanceTime);

    // Settings and Preferences
    Task<bool> UpdateNotificationPreferencesAsync(string userId, Dictionary<string, bool> preferences);
    Task<Dictionary<string, bool>> GetNotificationPreferencesAsync(string userId);
    Task<bool> IsNotificationEnabledAsync(string userId, string notificationType);

    // Cleanup and Maintenance
    Task<int> CleanupOldNotificationsAsync(int retentionDays = 30);
    Task<long> GetNotificationStorageSizeAsync();
} 