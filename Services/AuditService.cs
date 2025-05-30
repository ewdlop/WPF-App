using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net.NetworkInformation;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly List<AuditLog> _auditLogs = new();
    private int _nextId = 1;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Audit Logging Operations

    public async Task LogActionAsync(string tableName, int? recordId, AuditAction action, string userId, string userName,
                                    string? description = null, object? oldValues = null, object? newValues = null)
    {
        await Task.Delay(10);

        var auditLog = new AuditLog
        {
            Id = _nextId++,
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Description = description,
            OldValues = oldValues != null ? JsonConvert.SerializeObject(oldValues) : null,
            NewValues = newValues != null ? JsonConvert.SerializeObject(newValues) : null,
            IpAddress = GetLocalIPAddress(),
            UserAgent = "WPF Application",
            SessionId = Environment.MachineName + "_" + Environment.UserName,
            IsSuccessful = true,
            ApplicationVersion = "1.0.0",
            MachineName = Environment.MachineName
        };

        _auditLogs.Add(auditLog);
        _logger.LogInformation("Audit log created: {Action} on {TableName} by {UserName}", action, tableName, userName);
    }

    public async Task LogUserLoginAsync(string userId, string userName, bool isSuccessful, string? errorMessage = null)
    {
        await LogActionAsync("UserSessions", null, AuditAction.Login, userId, userName,
            description: isSuccessful ? "User logged in successfully" : $"Login failed: {errorMessage}");

        if (!isSuccessful)
        {
            var auditLog = _auditLogs.LastOrDefault();
            if (auditLog != null)
            {
                auditLog.IsSuccessful = false;
                auditLog.ErrorMessage = errorMessage;
            }
        }
    }

    public async Task LogUserLogoutAsync(string userId, string userName)
    {
        await LogActionAsync("UserSessions", null, AuditAction.Logout, userId, userName,
            description: "User logged out");
    }

    public async Task LogDataExportAsync(string userId, string userName, string dataType, string? description = null)
    {
        await LogActionAsync("DataExports", null, AuditAction.Export, userId, userName,
            description: description ?? $"Exported {dataType} data");
    }

    public async Task LogDataImportAsync(string userId, string userName, string dataType, int recordCount, string? description = null)
    {
        await LogActionAsync("DataImports", null, AuditAction.Import, userId, userName,
            description: description ?? $"Imported {recordCount} {dataType} records");
    }

    #endregion

    #region Audit Query Operations

    public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
    {
        await Task.Delay(50);
        return _auditLogs.OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId)
    {
        await Task.Delay(30);
        return _auditLogs.Where(a => a.UserId == userId)
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByActionAsync(AuditAction action)
    {
        await Task.Delay(30);
        return _auditLogs.Where(a => a.Action == action)
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByTableAsync(string tableName)
    {
        await Task.Delay(30);
        return _auditLogs.Where(a => a.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await Task.Delay(30);
        return _auditLogs.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsForRecordAsync(string tableName, int recordId)
    {
        await Task.Delay(20);
        return _auditLogs.Where(a => a.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) && 
                                    a.RecordId == recordId)
                        .OrderByDescending(a => a.Timestamp);
    }

    #endregion

    #region Search and Filter Operations

    public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(string searchTerm)
    {
        await Task.Delay(50);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAuditLogsAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        return _auditLogs.Where(a => 
            a.TableName.ToLowerInvariant().Contains(lowerSearchTerm) ||
            a.UserName.ToLowerInvariant().Contains(lowerSearchTerm) ||
            (a.Description?.ToLowerInvariant().Contains(lowerSearchTerm) ?? false) ||
            a.ActionDescription.ToLowerInvariant().Contains(lowerSearchTerm))
            .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100)
    {
        await Task.Delay(20);
        return _auditLogs.OrderByDescending(a => a.Timestamp)
                        .Take(count);
    }

    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync()
    {
        await Task.Delay(30);
        return _auditLogs.Where(a => !a.IsSuccessful)
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetSecurityEventsAsync()
    {
        await Task.Delay(30);
        var securityActions = new[] { AuditAction.Login, AuditAction.Logout };
        return _auditLogs.Where(a => securityActions.Contains(a.Action))
                        .OrderByDescending(a => a.Timestamp);
    }

    #endregion

    #region Statistics and Reports

    public async Task<int> GetTotalAuditLogCountAsync()
    {
        await Task.Delay(10);
        return _auditLogs.Count;
    }

    public async Task<Dictionary<AuditAction, int>> GetActionDistributionAsync()
    {
        await Task.Delay(30);
        return _auditLogs.GroupBy(a => a.Action)
                        .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetUserActivityDistributionAsync()
    {
        await Task.Delay(30);
        return _auditLogs.GroupBy(a => a.UserName)
                        .ToDictionary(g => g.Key, g => g.Count())
                        .OrderByDescending(kvp => kvp.Value)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<Dictionary<string, int>> GetTableAccessDistributionAsync()
    {
        await Task.Delay(30);
        return _auditLogs.GroupBy(a => a.TableName)
                        .ToDictionary(g => g.Key, g => g.Count())
                        .OrderByDescending(kvp => kvp.Value)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task<IEnumerable<AuditLog>> GetMostActiveUsersAsync(int count = 10)
    {
        await Task.Delay(30);
        var userActivityCounts = await GetUserActivityDistributionAsync();
        var topUsers = userActivityCounts.Take(count).Select(kvp => kvp.Key);
        
        return _auditLogs.Where(a => topUsers.Contains(a.UserName))
                        .OrderByDescending(a => a.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetSuspiciousActivitiesAsync()
    {
        await Task.Delay(50);
        var suspiciousLogs = new List<AuditLog>();

        // Failed login attempts
        suspiciousLogs.AddRange(_auditLogs.Where(a => a.Action == AuditAction.Login && !a.IsSuccessful));

        // Multiple rapid actions from same user
        var rapidActions = _auditLogs.GroupBy(a => new { a.UserId, Date = a.Timestamp.Date })
                                   .Where(g => g.Count() > 100) // More than 100 actions per day
                                   .SelectMany(g => g);
        suspiciousLogs.AddRange(rapidActions);

        // Actions outside business hours (assuming 9 AM - 6 PM)
        var outsideBusinessHours = _auditLogs.Where(a => a.Timestamp.Hour < 9 || a.Timestamp.Hour > 18);
        suspiciousLogs.AddRange(outsideBusinessHours);

        return suspiciousLogs.Distinct()
                           .OrderByDescending(a => a.Timestamp);
    }

    #endregion

    #region Cleanup and Maintenance

    public async Task<int> CleanupOldAuditLogsAsync(int retentionDays = 365)
    {
        await Task.Delay(50);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var oldLogs = _auditLogs.Where(a => a.Timestamp < cutoffDate).ToList();
        
        foreach (var log in oldLogs)
        {
            _auditLogs.Remove(log);
        }

        _logger.LogInformation("Cleaned up {Count} old audit logs older than {CutoffDate}", oldLogs.Count, cutoffDate);
        return oldLogs.Count;
    }

    public async Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate, string archivePath)
    {
        await Task.Delay(100);
        
        try
        {
            var logsToArchive = _auditLogs.Where(a => a.Timestamp < beforeDate).ToList();
            var json = JsonConvert.SerializeObject(logsToArchive, Formatting.Indented);
            
            var archiveFileName = Path.Combine(archivePath, $"audit_logs_{beforeDate:yyyyMMdd}.json");
            await File.WriteAllTextAsync(archiveFileName, json);
            
            // Remove archived logs from memory
            foreach (var log in logsToArchive)
            {
                _auditLogs.Remove(log);
            }

            _logger.LogInformation("Archived {Count} audit logs to {ArchiveFile}", logsToArchive.Count, archiveFileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive audit logs to {ArchivePath}", archivePath);
            return false;
        }
    }

    public async Task<long> GetAuditLogSizeAsync()
    {
        await Task.Delay(10);
        // Simple calculation based on log count
        return _auditLogs.Count * 2048; // Assume 2KB per audit log
    }

    #endregion

    #region Compliance and Security

    public async Task<bool> ValidateAuditIntegrityAsync()
    {
        await Task.Delay(50);
        
        // Basic integrity checks
        var duplicateIds = _auditLogs.GroupBy(a => a.Id)
                                   .Where(g => g.Count() > 1)
                                   .Any();
        
        if (duplicateIds)
        {
            _logger.LogWarning("Audit log integrity issue: Duplicate IDs found");
            return false;
        }

        var invalidTimestamps = _auditLogs.Where(a => a.Timestamp > DateTime.UtcNow || 
                                                     a.Timestamp < DateTime.UtcNow.AddYears(-10))
                                        .Any();
        
        if (invalidTimestamps)
        {
            _logger.LogWarning("Audit log integrity issue: Invalid timestamps found");
            return false;
        }

        _logger.LogInformation("Audit log integrity validation passed");
        return true;
    }

    public async Task<IEnumerable<AuditLog>> GetComplianceReportAsync(DateTime startDate, DateTime endDate)
    {
        await Task.Delay(50);
        
        // Get all audit logs in the specified period
        var complianceLogs = await GetAuditLogsByDateRangeAsync(startDate, endDate);
        
        _logger.LogInformation("Generated compliance report for period {StartDate} to {EndDate} with {Count} entries", 
            startDate, endDate, complianceLogs.Count());
        
        return complianceLogs;
    }

    public async Task<IEnumerable<AuditLog>> GetSecurityAuditReportAsync(DateTime startDate, DateTime endDate)
    {
        await Task.Delay(50);
        
        var securityActions = new[] { AuditAction.Login, AuditAction.Logout, AuditAction.Delete };
        var securityLogs = _auditLogs.Where(a => a.Timestamp >= startDate && 
                                               a.Timestamp <= endDate &&
                                               securityActions.Contains(a.Action))
                                   .OrderByDescending(a => a.Timestamp);
        
        _logger.LogInformation("Generated security audit report for period {StartDate} to {EndDate} with {Count} entries", 
            startDate, endDate, securityLogs.Count());
        
        return securityLogs;
    }

    #endregion

    #region Private Helper Methods

    private string GetLocalIPAddress()
    {
        try
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                     ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (networkInterface?.GetIPProperties().UnicastAddresses != null)
            {
                var ipAddress = networkInterface.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(ua => ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                
                return ipAddress?.Address.ToString() ?? "127.0.0.1";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local IP address");
        }
        
        return "127.0.0.1";
    }

    #endregion
} 