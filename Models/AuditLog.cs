using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp2.Models;

public enum AuditAction
{
    Create = 0,
    Read = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    Export = 6,
    Import = 7,
    Backup = 8,
    Restore = 9
}

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;

    public int? RecordId { get; set; }

    public AuditAction Action { get; set; }

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? AdditionalData { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    public bool IsSuccessful { get; set; } = true;

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    // Additional metadata
    [StringLength(50)]
    public string? ApplicationVersion { get; set; }

    [StringLength(100)]
    public string? MachineName { get; set; }

    public TimeSpan? ExecutionTime { get; set; }

    // Navigation properties
    public int? EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }

    // Computed properties
    [NotMapped]
    public string ActionDescription => Action switch
    {
        AuditAction.Create => "Created",
        AuditAction.Read => "Viewed",
        AuditAction.Update => "Modified",
        AuditAction.Delete => "Deleted",
        AuditAction.Login => "Logged in",
        AuditAction.Logout => "Logged out",
        AuditAction.Export => "Exported data",
        AuditAction.Import => "Imported data",
        AuditAction.Backup => "Created backup",
        AuditAction.Restore => "Restored data",
        _ => "Unknown action"
    };

    [NotMapped]
    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    [NotMapped]
    public string Summary => $"{UserName} {ActionDescription.ToLower()} {TableName}" +
                            (RecordId.HasValue ? $" (ID: {RecordId})" : "") +
                            $" at {FormattedTimestamp}";
} 