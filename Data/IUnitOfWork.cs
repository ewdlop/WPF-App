using WpfApp2.Data.Repositories;

namespace WpfApp2.Data;

public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IEmployeeRepository Employees { get; }
    IProjectRepository Projects { get; }
    IDepartmentRepository Departments { get; }
    IRepository<Models.AuditLog> AuditLogs { get; }
    IRepository<Models.ProjectAssignment> ProjectAssignments { get; }
    IRepository<Models.ProjectMilestone> ProjectMilestones { get; }

    // Transaction management
    Task<int> SaveChangesAsync();
    int SaveChanges();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    // Generic repository access
    IRepository<T> Repository<T>() where T : class;
} 