using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    // Department-specific queries
    Task<IEnumerable<Department>> GetActiveAsync();
    Task<IEnumerable<Department>> GetInactiveAsync();
    Task<IEnumerable<Department>> SearchAsync(string searchTerm);
    
    // Department operations
    Task<IEnumerable<Employee>> GetDepartmentEmployeesAsync(int departmentId);
    Task<IEnumerable<Project>> GetDepartmentProjectsAsync(int departmentId);
    Task<Dictionary<string, object>> GetDepartmentStatisticsAsync(int departmentId);
    Task<decimal> GetBudgetUtilizationAsync(int departmentId);
    
    // Business operations
    Task<bool> ActivateAsync(int departmentId);
    Task<bool> DeactivateAsync(int departmentId);
    Task<bool> CanDeleteAsync(int departmentId);
    
    // Validation
    Task<bool> IsCodeUniqueAsync(string code, int? excludeDepartmentId = null);
    Task<bool> IsNameUniqueAsync(string name, int? excludeDepartmentId = null);
    
    // Advanced queries
    Task<Department?> GetByCodeAsync(string code);
    Task<Department?> GetByNameAsync(string name);
    Task<IEnumerable<Department>> GetWithEmployeesAsync();
    Task<IEnumerable<Department>> GetWithProjectsAsync();
    Task<Department?> GetFullDepartmentAsync(int departmentId); // Includes all related data
    
    // Statistics
    Task<decimal> GetTotalBudgetAsync();
    Task<int> GetTotalEmployeeCountAsync();
    Task<int> GetActiveEmployeeCountAsync();
} 