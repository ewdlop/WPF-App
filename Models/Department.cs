using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp2.Models;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ManagerId { get; set; }

    [ForeignKey(nameof(ManagerId))]
    public virtual Employee? Manager { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Budget { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    // Computed properties
    [NotMapped]
    public int EmployeeCount => Employees?.Count(e => e.IsActive) ?? 0;

    [NotMapped]
    public decimal TotalSalaries => Employees?.Where(e => e.IsActive).Sum(e => e.Salary) ?? 0;

    [NotMapped]
    public decimal BudgetUtilization => Budget > 0 ? (TotalSalaries / Budget) * 100 : 0;
} 