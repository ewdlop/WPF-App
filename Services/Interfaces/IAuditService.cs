using WpfApp2.Models;

namespace WpfApp2.Services.Interfaces;

public interface IAuditService
{
    // Audit Logging Operations
    Task LogActionAsync(string tableName, int? recordId, AuditAction action, string userId, string userName, 
                       string? description = null, object? oldValues = null, object? newValues = null);
    Task LogUserLoginAsync(string userId, string userName, bool isSuccessful, string? errorMessage = null);
    Task LogUserLogoutAsync(string userId, string userName);
    Task LogDataExportAsync(string userId, string userName, string dataType, string? description = null);
    Task LogDataImportAsync(string userId, string userName, string dataType, int recordCount, string? description = null);

    // Audit Query Operations
    Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
    Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId);
    Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(AuditAction action);
    Task<IEnumerable<AuditLog>> GetAuditLogsByTableAsync(string tableName);
    Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetAuditLogsForRecordAsync(string tableName, int recordId);

    // Search and Filter Operations
    Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm);
    Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100);
    Task<IEnumerable<AuditLog>> GetFailedActionsAsync();
    Task<IEnumerable<AuditLog>> GetSecurityEventsAsync();

    // Statistics and Reports
    Task<int> GetTotalAuditLogCountAsync();
    Task<Dictionary<AuditAction, int>> GetActionDistributionAsync();
    Task<Dictionary<string, int>> GetUserActivityDistributionAsync();
    Task<Dictionary<string, int>> GetTableAccessDistributionAsync();
    Task<IEnumerable<AuditLog>> GetMostActiveUsersAsync(int count = 10);
    Task<IEnumerable<AuditLog>> GetSuspiciousActivitiesAsync();

    // Cleanup and Maintenance
    Task<int> CleanupOldAuditLogsAsync(int retentionDays = 365);
    Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate, string archivePath);
    Task<long> GetAuditLogSizeAsync();

    // Compliance and Security
    Task<bool> ValidateAuditIntegrityAsync();
    Task<IEnumerable<AuditLog>> GetComplianceReportAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AuditLog>> GetSecurityAuditReportAsync(DateTime startDate, DateTime endDate);
} 