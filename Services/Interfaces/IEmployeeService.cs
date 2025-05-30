using WpfApp2.Models;

namespace WpfApp2.Services.Interfaces;

public interface IEmployeeService
{
    // CRUD Operations
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<Employee> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);

    // Search and Filter Operations
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
    Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
    Task<IEnumerable<Employee>> GetInactiveEmployeesAsync();

    // Business Logic Operations
    Task<bool> ActivateEmployeeAsync(int id);
    Task<bool> DeactivateEmployeeAsync(int id);
    Task<bool> TransferEmployeeToDepartmentAsync(int employeeId, int newDepartmentId);
    Task<bool> UpdateEmployeeSalaryAsync(int employeeId, decimal newSalary);
    Task<bool> PromoteEmployeeAsync(int employeeId, string newPosition, decimal newSalary);

    // Statistics and Reports
    Task<int> GetTotalEmployeeCountAsync();
    Task<int> GetActiveEmployeeCountAsync();
    Task<decimal> GetAverageSalaryAsync();
    Task<decimal> GetAverageSalaryByDepartmentAsync(int departmentId);
    Task<IEnumerable<Employee>> GetTopPerformersAsync(int count = 10);
    Task<IEnumerable<Employee>> GetRecentHiresAsync(int days = 30);

    // Validation
    Task<bool> IsEmailUniqueAsync(string email, int? excludeEmployeeId = null);
    Task<bool> ValidateEmployeeDataAsync(Employee employee);
} 