using WpfApp2.Models;

namespace WpfApp2.Services.Interfaces;

public interface IDepartmentService
{
    // CRUD Operations
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department> CreateDepartmentAsync(Department department);
    Task<Department> UpdateDepartmentAsync(Department department);
    Task<bool> DeleteDepartmentAsync(int id);

    // Search and Filter Operations
    Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm);
    Task<IEnumerable<Department>> GetActiveDepartmentsAsync();
    Task<Department?> GetDepartmentByCodeAsync(string code);

    // Department Management Operations
    Task<bool> SetDepartmentManagerAsync(int departmentId, int managerId);
    Task<bool> UpdateDepartmentBudgetAsync(int departmentId, decimal newBudget);
    Task<bool> ActivateDepartmentAsync(int departmentId);
    Task<bool> DeactivateDepartmentAsync(int departmentId);

    // Employee Operations
    Task<IEnumerable<Employee>> GetDepartmentEmployeesAsync(int departmentId);
    Task<bool> TransferEmployeeToDepartmentAsync(int employeeId, int newDepartmentId);
    Task<int> GetEmployeeCountAsync(int departmentId);

    // Project Operations
    Task<IEnumerable<Project>> GetDepartmentProjectsAsync(int departmentId);
    Task<int> GetActiveProjectCountAsync(int departmentId);

    // Statistics and Reports
    Task<decimal> GetDepartmentTotalSalariesAsync(int departmentId);
    Task<decimal> GetDepartmentBudgetUtilizationAsync(int departmentId);
    Task<decimal> GetAverageSalaryAsync(int departmentId);
    Task<Dictionary<string, object>> GetDepartmentStatisticsAsync(int departmentId);
    Task<IEnumerable<Department>> GetDepartmentsByBudgetRangeAsync(decimal minBudget, decimal maxBudget);

    // Validation
    Task<bool> IsDepartmentCodeUniqueAsync(string code, int? excludeDepartmentId = null);
    Task<bool> CanDeleteDepartmentAsync(int departmentId);
    Task<bool> ValidateDepartmentDataAsync(Department department);
} 