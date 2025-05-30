using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp2.Models;

namespace WpfApp2.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger) 
        : base(options)
    {
        _logger = logger;
    }

    // DbSets for all entities
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; } = null!;
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Employee entity
        ConfigureEmployee(modelBuilder);
        
        // Configure Department entity
        ConfigureDepartment(modelBuilder);
        
        // Configure Project entity
        ConfigureProject(modelBuilder);
        
        // Configure ProjectAssignment entity
        ConfigureProjectAssignment(modelBuilder);
        
        // Configure ProjectMilestone entity
        ConfigureProjectMilestone(modelBuilder);
        
        // Configure AuditLog entity
        ConfigureAuditLog(modelBuilder);

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void ConfigureEmployee(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Employee>();

        // Table configuration
        entity.ToTable("Employees");

        // Primary key
        entity.HasKey(e => e.Id);

        // Properties
        entity.Property(e => e.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.PhoneNumber)
            .HasMaxLength(20);

        entity.Property(e => e.Position)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(e => e.HireDate)
            .IsRequired();

        entity.Property(e => e.TerminationDate)
            .IsRequired(false);

        entity.Property(e => e.DateOfBirth)
            .IsRequired(false);

        entity.Property(e => e.Address)
            .HasMaxLength(500);

        entity.Property(e => e.EmergencyContact)
            .HasMaxLength(255);

        entity.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(e => e.Notes)
            .HasMaxLength(500);

        entity.Property(e => e.ProfileImagePath)
            .HasMaxLength(255);

        entity.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        entity.HasIndex(e => e.EmployeeNumber)
            .IsUnique()
            .HasDatabaseName("IX_Employees_EmployeeNumber");

        entity.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_Employees_Email");

        entity.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        entity.HasIndex(e => e.ManagerId)
            .HasDatabaseName("IX_Employees_ManagerId");

        entity.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Employees_IsActive");

        entity.HasIndex(e => e.HireDate)
            .HasDatabaseName("IX_Employees_HireDate");

        // Relationships
        entity.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Department>();

        entity.ToTable("Departments");
        entity.HasKey(d => d.Id);

        entity.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(10);

        entity.Property(d => d.Description)
            .HasMaxLength(500);

        entity.Property(d => d.Budget)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(d => d.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(d => d.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        entity.HasIndex(d => d.Code)
            .IsUnique()
            .HasDatabaseName("IX_Departments_Code");

        entity.HasIndex(d => d.Name)
            .HasDatabaseName("IX_Departments_Name");

        entity.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Departments_IsActive");

        entity.HasIndex(d => d.ManagerId)
            .HasDatabaseName("IX_Departments_ManagerId");

        // Self-referencing relationship for manager
        entity.HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureProject(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Project>();

        entity.ToTable("Projects");
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(p => p.Description)
            .HasMaxLength(1000);

        entity.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        entity.Property(p => p.Priority)
            .IsRequired()
            .HasConversion<string>();

        entity.Property(p => p.StartDate)
            .IsRequired();

        entity.Property(p => p.EndDate)
            .IsRequired(false);

        entity.Property(p => p.EstimatedEndDate)
            .IsRequired();

        entity.Property(p => p.Budget)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(p => p.ActualCost)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.Property(p => p.ProgressPercentage)
            .IsRequired()
            .HasDefaultValue(0);

        entity.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(p => p.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(p => p.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        entity.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("IX_Projects_Code");

        entity.HasIndex(p => p.DepartmentId)
            .HasDatabaseName("IX_Projects_DepartmentId");

        entity.HasIndex(p => p.ProjectManagerId)
            .HasDatabaseName("IX_Projects_ProjectManagerId");

        entity.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Projects_Status");

        entity.HasIndex(p => p.Priority)
            .HasDatabaseName("IX_Projects_Priority");

        entity.HasIndex(p => p.StartDate)
            .HasDatabaseName("IX_Projects_StartDate");

        entity.HasIndex(p => p.EstimatedEndDate)
            .HasDatabaseName("IX_Projects_EstimatedEndDate");

        // Relationships
        entity.HasOne(p => p.Department)
            .WithMany(d => d.Projects)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(p => p.ProjectManager)
            .WithMany()
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureProjectAssignment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ProjectAssignment>();

        entity.ToTable("ProjectAssignments");
        entity.HasKey(pa => pa.Id);

        entity.Property(pa => pa.Role)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(pa => pa.HourlyRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        entity.Property(pa => pa.AssignedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(pa => pa.UnassignedDate)
            .IsRequired(false);

        entity.Property(pa => pa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(pa => pa.AllocationPercentage)
            .IsRequired()
            .HasDefaultValue(100);

        entity.Property(pa => pa.Notes)
            .HasMaxLength(500);

        entity.Property(pa => pa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(pa => pa.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(pa => pa.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(pa => pa.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        entity.HasIndex(pa => pa.ProjectId)
            .HasDatabaseName("IX_ProjectAssignments_ProjectId");

        entity.HasIndex(pa => pa.EmployeeId)
            .HasDatabaseName("IX_ProjectAssignments_EmployeeId");

        entity.HasIndex(pa => pa.IsActive)
            .HasDatabaseName("IX_ProjectAssignments_IsActive");

        entity.HasIndex(pa => pa.AssignedDate)
            .HasDatabaseName("IX_ProjectAssignments_AssignedDate");

        // Composite index for unique assignment
        entity.HasIndex(pa => new { pa.ProjectId, pa.EmployeeId })
            .IsUnique()
            .HasDatabaseName("IX_ProjectAssignments_Project_Employee");

        // Relationships
        entity.HasOne(pa => pa.Project)
            .WithMany(p => p.ProjectAssignments)
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(pa => pa.Employee)
            .WithMany(e => e.ProjectAssignments)
            .HasForeignKey(pa => pa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureProjectMilestone(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ProjectMilestone>();

        entity.ToTable("ProjectMilestones");
        entity.HasKey(pm => pm.Id);

        entity.Property(pm => pm.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(pm => pm.Description)
            .HasMaxLength(500);

        entity.Property(pm => pm.DueDate)
            .IsRequired();

        entity.Property(pm => pm.CompletedDate)
            .IsRequired(false);

        entity.Property(pm => pm.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(pm => pm.IsCritical)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(pm => pm.ProgressPercentage)
            .IsRequired()
            .HasDefaultValue(0);

        entity.Property(pm => pm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(pm => pm.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(pm => pm.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(pm => pm.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Indexes
        entity.HasIndex(pm => pm.ProjectId)
            .HasDatabaseName("IX_ProjectMilestones_ProjectId");

        entity.HasIndex(pm => pm.DueDate)
            .HasDatabaseName("IX_ProjectMilestones_DueDate");

        entity.HasIndex(pm => pm.IsCompleted)
            .HasDatabaseName("IX_ProjectMilestones_IsCompleted");

        entity.HasIndex(pm => pm.IsCritical)
            .HasDatabaseName("IX_ProjectMilestones_IsCritical");

        // Relationships
        entity.HasOne(pm => pm.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuditLog>();

        entity.ToTable("AuditLogs");
        entity.HasKey(al => al.Id);

        entity.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(al => al.EntityId)
            .IsRequired(false);

        entity.Property(al => al.TableName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(al => al.RecordId)
            .IsRequired(false);

        entity.Property(al => al.Action)
            .IsRequired()
            .HasConversion<string>();

        entity.Property(al => al.UserId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(al => al.UserName)
            .HasMaxLength(100);

        entity.Property(al => al.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        entity.Property(al => al.Description)
            .HasMaxLength(500);

        entity.Property(al => al.OldValues)
            .HasColumnType("nvarchar(max)");

        entity.Property(al => al.NewValues)
            .HasColumnType("nvarchar(max)");

        entity.Property(al => al.AdditionalData)
            .HasColumnType("nvarchar(max)");

        entity.Property(al => al.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        entity.Property(al => al.UserAgent)
            .HasMaxLength(500);

        entity.Property(al => al.SessionId)
            .HasMaxLength(100);

        entity.Property(al => al.IsSuccessful)
            .IsRequired()
            .HasDefaultValue(true);

        entity.Property(al => al.ErrorMessage)
            .HasMaxLength(1000);

        entity.Property(al => al.ApplicationVersion)
            .HasMaxLength(50);

        entity.Property(al => al.MachineName)
            .HasMaxLength(100);

        entity.Property(al => al.ExecutionTime)
            .IsRequired(false);

        // Indexes
        entity.HasIndex(al => al.EntityType)
            .HasDatabaseName("IX_AuditLogs_EntityType");

        entity.HasIndex(al => al.EntityId)
            .HasDatabaseName("IX_AuditLogs_EntityId");

        entity.HasIndex(al => al.TableName)
            .HasDatabaseName("IX_AuditLogs_TableName");

        entity.HasIndex(al => al.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        entity.HasIndex(al => al.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        entity.HasIndex(al => al.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        entity.HasIndex(al => al.IsSuccessful)
            .HasDatabaseName("IX_AuditLogs_IsSuccessful");

        // Relationship with Employee (optional)
        entity.HasOne(al => al.Employee)
            .WithMany(e => e.AuditLogs)
            .HasForeignKey(al => al.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Departments
        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Id = 1,
                Name = "Information Technology",
                Code = "IT",
                Description = "Responsible for all technology infrastructure and software development",
                Budget = 500000m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Department
            {
                Id = 2,
                Name = "Human Resources",
                Code = "HR",
                Description = "Manages employee relations, recruitment, and organizational development",
                Budget = 200000m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Department
            {
                Id = 3,
                Name = "Finance",
                Code = "FIN",
                Description = "Handles financial planning, accounting, and budget management",
                Budget = 300000m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Department
            {
                Id = 4,
                Name = "Marketing",
                Code = "MKT",
                Description = "Responsible for marketing strategies and customer engagement",
                Budget = 250000m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        );

        // Seed Employees
        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                EmployeeNumber = "EMP001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                PhoneNumber = "+1-555-0101",
                Position = "Software Engineer",
                DepartmentId = 1,
                Salary = 75000m,
                HireDate = DateTime.UtcNow.AddDays(-365),
                IsActive = true,
                Address = "123 Main St, City, State 12345",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                EmergencyContact = "Jane Doe - 555-0102",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Employee
            {
                Id = 2,
                EmployeeNumber = "EMP002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                PhoneNumber = "+1-555-0103",
                Position = "Project Manager",
                DepartmentId = 1,
                Salary = 85000m,
                HireDate = DateTime.UtcNow.AddDays(-200),
                IsActive = true,
                Address = "456 Oak Ave, City, State 12345",
                DateOfBirth = DateTime.UtcNow.AddYears(-32),
                EmergencyContact = "John Smith - 555-0104",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Employee
            {
                Id = 3,
                EmployeeNumber = "EMP003",
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@company.com",
                PhoneNumber = "+1-555-0105",
                Position = "HR Manager",
                DepartmentId = 2,
                Salary = 70000m,
                HireDate = DateTime.UtcNow.AddDays(-150),
                IsActive = true,
                Address = "789 Pine St, City, State 12345",
                DateOfBirth = DateTime.UtcNow.AddYears(-35),
                EmergencyContact = "Alice Johnson - 555-0106",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Employee
            {
                Id = 4,
                EmployeeNumber = "EMP004",
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@company.com",
                PhoneNumber = "+1-555-0107",
                Position = "Senior Developer",
                DepartmentId = 1,
                Salary = 95000m,
                HireDate = DateTime.UtcNow.AddDays(-500),
                IsActive = true,
                Address = "321 Elm Dr, City, State 12345",
                DateOfBirth = DateTime.UtcNow.AddYears(-28),
                EmergencyContact = "Tom Brown - 555-0108",
                ManagerId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Employee
            {
                Id = 5,
                EmployeeNumber = "EMP005",
                FirstName = "Charlie",
                LastName = "Wilson",
                Email = "charlie.wilson@company.com",
                PhoneNumber = "+1-555-0109",
                Position = "Financial Analyst",
                DepartmentId = 3,
                Salary = 60000m,
                HireDate = DateTime.UtcNow.AddDays(-100),
                IsActive = true,
                Address = "654 Maple Ln, City, State 12345",
                DateOfBirth = DateTime.UtcNow.AddYears(-26),
                EmergencyContact = "Sarah Wilson - 555-0110",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        );

        // Seed Projects
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = 1,
                Name = "Enterprise Portal Development",
                Code = "EPD2024",
                Description = "Development of a comprehensive enterprise portal for internal operations",
                Status = ProjectStatus.InProgress,
                Priority = ProjectPriority.High,
                Budget = 150000m,
                ActualCost = 75000m,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(90),
                EstimatedEndDate = DateTime.UtcNow.AddDays(85),
                ProjectManagerId = 2,
                DepartmentId = 1,
                ProgressPercentage = 50,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new Project
            {
                Id = 2,
                Name = "HR System Modernization",
                Code = "HSM2024",
                Description = "Modernization of the human resources management system",
                Status = ProjectStatus.Planning,
                Priority = ProjectPriority.Medium,
                Budget = 80000m,
                ActualCost = 10000m,
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(180),
                EstimatedEndDate = DateTime.UtcNow.AddDays(175),
                ProjectManagerId = 2,
                DepartmentId = 2,
                ProgressPercentage = 10,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (_logger != null)
        {
            optionsBuilder.LogTo(message => _logger.LogInformation(message))
                         .EnableSensitiveDataLogging()
                         .EnableDetailedErrors();
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set audit fields
        SetAuditFields();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Automatically set audit fields
        SetAuditFields();
        
        return base.SaveChanges();
    }

    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Employee employee)
            {
                if (entry.State == EntityState.Added)
                {
                    employee.CreatedAt = DateTime.UtcNow;
                    employee.CreatedBy ??= "System";
                }
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy ??= "System";
            }
            else if (entry.Entity is Department department)
            {
                if (entry.State == EntityState.Added)
                {
                    department.CreatedAt = DateTime.UtcNow;
                    department.CreatedBy ??= "System";
                }
                department.UpdatedAt = DateTime.UtcNow;
                department.UpdatedBy ??= "System";
            }
            else if (entry.Entity is Project project)
            {
                if (entry.State == EntityState.Added)
                {
                    project.CreatedAt = DateTime.UtcNow;
                    project.CreatedBy ??= "System";
                }
                project.UpdatedAt = DateTime.UtcNow;
                project.UpdatedBy ??= "System";
            }
            else if (entry.Entity is ProjectAssignment assignment)
            {
                if (entry.State == EntityState.Added)
                {
                    assignment.CreatedAt = DateTime.UtcNow;
                    assignment.CreatedBy ??= "System";
                }
                assignment.UpdatedAt = DateTime.UtcNow;
                assignment.UpdatedBy ??= "System";
            }
            else if (entry.Entity is ProjectMilestone milestone)
            {
                if (entry.State == EntityState.Added)
                {
                    milestone.CreatedAt = DateTime.UtcNow;
                    milestone.CreatedBy ??= "System";
                }
                milestone.UpdatedAt = DateTime.UtcNow;
                milestone.UpdatedBy ??= "System";
            }
        }
    }
} 