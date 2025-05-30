using Microsoft.Extensions.Logging;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ILogger<EmployeeService> _logger;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // In-memory storage for demonstration (replace with actual repository in production)
    private readonly List<Employee> _employees = new();
    private int _nextId = 1;

    public EmployeeService(ILogger<EmployeeService> logger, IAuditService auditService, INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Initialize with sample data
        InitializeSampleData();
    }

    #region CRUD Operations

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        _logger.LogInformation("Retrieving all employees");
        await Task.Delay(100); // Simulate async operation
        return _employees.ToList();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving employee with ID: {EmployeeId}", id);
        await Task.Delay(50);
        return _employees.FirstOrDefault(e => e.Id == id);
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        _logger.LogInformation("Creating new employee: {EmployeeName}", employee.FullName);
        
        // Validate employee data
        if (!await ValidateEmployeeDataAsync(employee))
        {
            throw new ArgumentException("Invalid employee data");
        }

        // Set auto-generated fields
        employee.Id = _nextId++;
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.CreatedBy = "System"; // In real app, get from current user context
        employee.UpdatedBy = "System";

        _employees.Add(employee);

        // Log audit trail
        await _auditService.LogActionAsync("Employees", employee.Id, AuditAction.Create, "System", "System User", 
            description: $"Created employee: {employee.FullName}");

        // Send notification
        await _notificationService.NotifyEmployeeCreatedAsync(employee.Id);

        _logger.LogInformation("Employee created successfully: {EmployeeId}", employee.Id);
        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        _logger.LogInformation("Updating employee: {EmployeeId}", employee.Id);
        
        var existingEmployee = await GetEmployeeByIdAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new ArgumentException($"Employee with ID {employee.Id} not found");
        }

        if (!await ValidateEmployeeDataAsync(employee))
        {
            throw new ArgumentException("Invalid employee data");
        }

        // Store old values for audit
        var oldValues = new { existingEmployee.FirstName, existingEmployee.LastName, existingEmployee.Email, existingEmployee.Position, existingEmployee.Salary };

        // Update fields
        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.Email = employee.Email;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Position = employee.Position;
        existingEmployee.DepartmentId = employee.DepartmentId;
        existingEmployee.Salary = employee.Salary;
        existingEmployee.Notes = employee.Notes;
        existingEmployee.UpdatedAt = DateTime.UtcNow;
        existingEmployee.UpdatedBy = "System";

        // Log audit trail
        var newValues = new { existingEmployee.FirstName, existingEmployee.LastName, existingEmployee.Email, existingEmployee.Position, existingEmployee.Salary };
        await _auditService.LogActionAsync("Employees", employee.Id, AuditAction.Update, "System", "System User", 
            description: $"Updated employee: {existingEmployee.FullName}", oldValues: oldValues, newValues: newValues);

        // Send notification
        await _notificationService.NotifyEmployeeUpdatedAsync(employee.Id);

        _logger.LogInformation("Employee updated successfully: {EmployeeId}", employee.Id);
        return existingEmployee;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        _logger.LogInformation("Deleting employee: {EmployeeId}", id);
        
        var employee = await GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return false;
        }

        _employees.Remove(employee);

        // Log audit trail
        await _auditService.LogActionAsync("Employees", id, AuditAction.Delete, "System", "System User", 
            description: $"Deleted employee: {employee.FullName}");

        _logger.LogInformation("Employee deleted successfully: {EmployeeId}", id);
        return true;
    }

    #endregion

    #region Search and Filter Operations

    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
    {
        _logger.LogInformation("Searching employees with term: {SearchTerm}", searchTerm);
        await Task.Delay(100);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllEmployeesAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        return _employees.Where(e => 
            e.FirstName.ToLowerInvariant().Contains(lowerSearchTerm) ||
            e.LastName.ToLowerInvariant().Contains(lowerSearchTerm) ||
            e.Email.ToLowerInvariant().Contains(lowerSearchTerm) ||
            e.Position.ToLowerInvariant().Contains(lowerSearchTerm));
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        _logger.LogInformation("Retrieving employees for department: {DepartmentId}", departmentId);
        await Task.Delay(50);
        return _employees.Where(e => e.DepartmentId == departmentId && e.IsActive);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        _logger.LogInformation("Retrieving active employees");
        await Task.Delay(50);
        return _employees.Where(e => e.IsActive);
    }

    public async Task<IEnumerable<Employee>> GetInactiveEmployeesAsync()
    {
        _logger.LogInformation("Retrieving inactive employees");
        await Task.Delay(50);
        return _employees.Where(e => !e.IsActive);
    }

    #endregion

    #region Business Logic Operations

    public async Task<bool> ActivateEmployeeAsync(int id)
    {
        var employee = await GetEmployeeByIdAsync(id);
        if (employee == null) return false;

        employee.IsActive = true;
        employee.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Employees", id, AuditAction.Update, "System", "System User", 
            description: $"Activated employee: {employee.FullName}");

        return true;
    }

    public async Task<bool> DeactivateEmployeeAsync(int id)
    {
        var employee = await GetEmployeeByIdAsync(id);
        if (employee == null) return false;

        employee.IsActive = false;
        employee.TerminationDate = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Employees", id, AuditAction.Update, "System", "System User", 
            description: $"Deactivated employee: {employee.FullName}");

        await _notificationService.NotifyEmployeeDeactivatedAsync(id);

        return true;
    }

    public async Task<bool> TransferEmployeeToDepartmentAsync(int employeeId, int newDepartmentId)
    {
        var employee = await GetEmployeeByIdAsync(employeeId);
        if (employee == null) return false;

        var oldDepartmentId = employee.DepartmentId;
        employee.DepartmentId = newDepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Employees", employeeId, AuditAction.Update, "System", "System User", 
            description: $"Transferred employee {employee.FullName} from department {oldDepartmentId} to {newDepartmentId}");

        return true;
    }

    public async Task<bool> UpdateEmployeeSalaryAsync(int employeeId, decimal newSalary)
    {
        var employee = await GetEmployeeByIdAsync(employeeId);
        if (employee == null) return false;

        var oldSalary = employee.Salary;
        employee.Salary = newSalary;
        employee.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Employees", employeeId, AuditAction.Update, "System", "System User", 
            description: $"Updated salary for {employee.FullName} from {oldSalary:C} to {newSalary:C}");

        return true;
    }

    public async Task<bool> PromoteEmployeeAsync(int employeeId, string newPosition, decimal newSalary)
    {
        var employee = await GetEmployeeByIdAsync(employeeId);
        if (employee == null) return false;

        var oldPosition = employee.Position;
        var oldSalary = employee.Salary;
        
        employee.Position = newPosition;
        employee.Salary = newSalary;
        employee.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Employees", employeeId, AuditAction.Update, "System", "System User", 
            description: $"Promoted {employee.FullName} from {oldPosition} to {newPosition}, salary updated from {oldSalary:C} to {newSalary:C}");

        return true;
    }

    #endregion

    #region Statistics and Reports

    public async Task<int> GetTotalEmployeeCountAsync()
    {
        await Task.Delay(10);
        return _employees.Count;
    }

    public async Task<int> GetActiveEmployeeCountAsync()
    {
        await Task.Delay(10);
        return _employees.Count(e => e.IsActive);
    }

    public async Task<decimal> GetAverageSalaryAsync()
    {
        await Task.Delay(10);
        var activeEmployees = _employees.Where(e => e.IsActive);
        return activeEmployees.Any() ? activeEmployees.Average(e => e.Salary) : 0;
    }

    public async Task<decimal> GetAverageSalaryByDepartmentAsync(int departmentId)
    {
        await Task.Delay(10);
        var departmentEmployees = _employees.Where(e => e.DepartmentId == departmentId && e.IsActive);
        return departmentEmployees.Any() ? departmentEmployees.Average(e => e.Salary) : 0;
    }

    public async Task<IEnumerable<Employee>> GetTopPerformersAsync(int count = 10)
    {
        await Task.Delay(50);
        // In a real application, this would be based on performance metrics
        return _employees.Where(e => e.IsActive)
                        .OrderByDescending(e => e.Salary)
                        .Take(count);
    }

    public async Task<IEnumerable<Employee>> GetRecentHiresAsync(int days = 30)
    {
        await Task.Delay(50);
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return _employees.Where(e => e.HireDate >= cutoffDate)
                        .OrderByDescending(e => e.HireDate);
    }

    #endregion

    #region Validation

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeEmployeeId = null)
    {
        await Task.Delay(10);
        return !_employees.Any(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                                   (!excludeEmployeeId.HasValue || e.Id != excludeEmployeeId.Value));
    }

    public async Task<bool> ValidateEmployeeDataAsync(Employee employee)
    {
        await Task.Delay(10);
        
        if (string.IsNullOrWhiteSpace(employee.FirstName) || 
            string.IsNullOrWhiteSpace(employee.LastName) ||
            string.IsNullOrWhiteSpace(employee.Email) ||
            string.IsNullOrWhiteSpace(employee.Position))
        {
            return false;
        }

        if (!await IsEmailUniqueAsync(employee.Email, employee.Id > 0 ? employee.Id : null))
        {
            return false;
        }

        if (employee.Salary < 0 || employee.DepartmentId <= 0)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods

    private void InitializeSampleData()
    {
        var sampleEmployees = new[]
        {
            new Employee { Id = _nextId++, FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Position = "Software Engineer", DepartmentId = 1, Salary = 75000, HireDate = DateTime.UtcNow.AddDays(-365), IsActive = true },
            new Employee { Id = _nextId++, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Position = "Project Manager", DepartmentId = 2, Salary = 85000, HireDate = DateTime.UtcNow.AddDays(-200), IsActive = true },
            new Employee { Id = _nextId++, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@company.com", Position = "Designer", DepartmentId = 3, Salary = 65000, HireDate = DateTime.UtcNow.AddDays(-150), IsActive = true },
            new Employee { Id = _nextId++, FirstName = "Alice", LastName = "Brown", Email = "alice.brown@company.com", Position = "Senior Developer", DepartmentId = 1, Salary = 95000, HireDate = DateTime.UtcNow.AddDays(-500), IsActive = true },
            new Employee { Id = _nextId++, FirstName = "Charlie", LastName = "Wilson", Email = "charlie.wilson@company.com", Position = "QA Engineer", DepartmentId = 1, Salary = 60000, HireDate = DateTime.UtcNow.AddDays(-100), IsActive = false }
        };

        _employees.AddRange(sampleEmployees);
        _logger.LogInformation("Initialized with {Count} sample employees", sampleEmployees.Length);
    }

    #endregion
} 