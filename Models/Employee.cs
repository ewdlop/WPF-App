using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp2.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(255)]
    public string? ProfileImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    // Computed properties
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [NotMapped]
    public int YearsOfService => 
        (int)((TerminationDate ?? DateTime.Now) - HireDate).TotalDays / 365;

    [NotMapped]
    public string Status => IsActive ? "Active" : "Inactive";
} 