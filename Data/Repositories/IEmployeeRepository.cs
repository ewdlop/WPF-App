using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    // Employee-specific queries
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    Task<IEnumerable<Employee>> GetInactiveEmployeesAsync();
    Task<IEnumerable<Employee>> GetByManagerAsync(int managerId);
    Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
    
    // Validation methods
    Task<bool> IsEmailUniqueAsync(string email, int? excludeEmployeeId = null);
    Task<bool> IsEmployeeNumberUniqueAsync(string employeeNumber, int? excludeEmployeeId = null);
    
    // Business operations
    Task<bool> ActivateAsync(int employeeId);
    Task<bool> DeactivateAsync(int employeeId);
    Task<bool> TransferToDepartmentAsync(int employeeId, int newDepartmentId);
    Task<bool> UpdateSalaryAsync(int employeeId, decimal newSalary);
    Task<bool> PromoteAsync(int employeeId, string newPosition, decimal newSalary);
    
    // Statistics
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task<decimal> GetAverageSalaryAsync();
    Task<decimal> GetAverageSalaryByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetTopPerformersAsync(int count = 10);
    Task<IEnumerable<Employee>> GetRecentHiresAsync(int days = 30);
    
    // Advanced queries
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<Employee?> GetByEmailAsync(string email);
    Task<IEnumerable<Employee>> GetWithProjectsAsync();
    Task<IEnumerable<Employee>> GetManagersAsync();
} 