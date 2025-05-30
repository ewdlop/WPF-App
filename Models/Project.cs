using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp2.Models;

public enum ProjectStatus
{
    Planning = 0,
    InProgress = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

public enum ProjectPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class Project
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime EstimatedEndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; }

    [Range(0, 100)]
    public int ProgressPercentage { get; set; }

    public int DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;

    public int? ProjectManagerId { get; set; }

    [ForeignKey(nameof(ProjectManagerId))]
    public virtual Employee? ProjectManager { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();

    // Computed properties
    [NotMapped]
    public bool IsOverdue => EstimatedEndDate < DateTime.Now && Status != ProjectStatus.Completed;

    [NotMapped]
    public bool IsOverBudget => ActualCost > Budget;

    [NotMapped]
    public int DaysRemaining => EstimatedEndDate > DateTime.Now ? 
        (int)(EstimatedEndDate - DateTime.Now).TotalDays : 0;

    [NotMapped]
    public decimal BudgetUtilization => Budget > 0 ? (ActualCost / Budget) * 100 : 0;

    [NotMapped]
    public int TeamSize => ProjectAssignments?.Count(pa => pa.IsActive) ?? 0;
}

public class ProjectAssignment
{
    [Key]
    public int Id { get; set; }

    public int ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; } = null!;

    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UnassignedDate { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, 100)]
    public int AllocationPercentage { get; set; } = 100;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
}

public class ProjectMilestone
{
    [Key]
    public int Id { get; set; }

    public int ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsCritical { get; set; }

    [Range(0, 100)]
    public int ProgressPercentage { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Computed properties
    [NotMapped]
    public bool IsOverdue => DueDate < DateTime.Now && !IsCompleted;

    [NotMapped]
    public int DaysUntilDue => DueDate > DateTime.Now ? 
        (int)(DueDate - DateTime.Now).TotalDays : 0;
} 