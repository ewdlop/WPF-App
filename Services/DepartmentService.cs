using Microsoft.Extensions.Logging;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ILogger<DepartmentService> _logger;
    private readonly IAuditService _auditService;
    private readonly IEmployeeService _employeeService;
    private readonly IProjectService _projectService;

    // In-memory storage for demonstration
    private readonly List<Department> _departments = new();
    private int _nextId = 1;

    public DepartmentService(ILogger<DepartmentService> logger, IAuditService auditService, 
                           IEmployeeService employeeService, IProjectService projectService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));

        InitializeSampleData();
    }

    #region CRUD Operations

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        _logger.LogInformation("Retrieving all departments");
        await Task.Delay(100);
        return _departments.ToList();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving department with ID: {DepartmentId}", id);
        await Task.Delay(50);
        return _departments.FirstOrDefault(d => d.Id == id);
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        _logger.LogInformation("Creating new department: {DepartmentName}", department.Name);
        
        if (!await ValidateDepartmentDataAsync(department))
        {
            throw new ArgumentException("Invalid department data");
        }

        department.Id = _nextId++;
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;

        _departments.Add(department);

        await _auditService.LogActionAsync("Departments", department.Id, AuditAction.Create, "System", "System User", 
            description: $"Created department: {department.Name}");

        _logger.LogInformation("Department created successfully: {DepartmentId}", department.Id);
        return department;
    }

    public async Task<Department> UpdateDepartmentAsync(Department department)
    {
        _logger.LogInformation("Updating department: {DepartmentId}", department.Id);
        
        var existingDepartment = await GetDepartmentByIdAsync(department.Id);
        if (existingDepartment == null)
        {
            throw new ArgumentException($"Department with ID {department.Id} not found");
        }

        if (!await ValidateDepartmentDataAsync(department))
        {
            throw new ArgumentException("Invalid department data");
        }

        var oldValues = new { existingDepartment.Name, existingDepartment.Code, existingDepartment.Budget, existingDepartment.ManagerId };

        existingDepartment.Name = department.Name;
        existingDepartment.Code = department.Code;
        existingDepartment.Description = department.Description;
        existingDepartment.Budget = department.Budget;
        existingDepartment.ManagerId = department.ManagerId;
        existingDepartment.IsActive = department.IsActive;
        existingDepartment.UpdatedAt = DateTime.UtcNow;

        var newValues = new { existingDepartment.Name, existingDepartment.Code, existingDepartment.Budget, existingDepartment.ManagerId };

        await _auditService.LogActionAsync("Departments", department.Id, AuditAction.Update, "System", "System User", 
            description: $"Updated department: {existingDepartment.Name}", oldValues: oldValues, newValues: newValues);

        _logger.LogInformation("Department updated successfully: {DepartmentId}", department.Id);
        return existingDepartment;
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        _logger.LogInformation("Deleting department: {DepartmentId}", id);
        
        if (!await CanDeleteDepartmentAsync(id))
        {
            return false;
        }

        var department = await GetDepartmentByIdAsync(id);
        if (department == null)
        {
            return false;
        }

        _departments.Remove(department);

        await _auditService.LogActionAsync("Departments", id, AuditAction.Delete, "System", "System User", 
            description: $"Deleted department: {department.Name}");

        _logger.LogInformation("Department deleted successfully: {DepartmentId}", id);
        return true;
    }

    #endregion

    #region Search and Filter Operations

    public async Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        _logger.LogInformation("Searching departments with term: {SearchTerm}", searchTerm);
        await Task.Delay(100);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllDepartmentsAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        return _departments.Where(d => 
            d.Name.ToLowerInvariant().Contains(lowerSearchTerm) ||
            d.Code.ToLowerInvariant().Contains(lowerSearchTerm) ||
            d.Description?.ToLowerInvariant().Contains(lowerSearchTerm) == true);
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
    {
        _logger.LogInformation("Retrieving active departments");
        await Task.Delay(50);
        return _departments.Where(d => d.IsActive);
    }

    public async Task<Department?> GetDepartmentByCodeAsync(string code)
    {
        _logger.LogInformation("Retrieving department with code: {DepartmentCode}", code);
        await Task.Delay(50);
        return _departments.FirstOrDefault(d => d.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Department Management Operations

    public async Task<bool> SetDepartmentManagerAsync(int departmentId, int managerId)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null) return false;

        var oldManagerId = department.ManagerId;
        department.ManagerId = managerId;
        department.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Departments", departmentId, AuditAction.Update, "System", "System User", 
            description: $"Changed department manager from {oldManagerId} to {managerId}");

        return true;
    }

    public async Task<bool> UpdateDepartmentBudgetAsync(int departmentId, decimal newBudget)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null || newBudget < 0) return false;

        var oldBudget = department.Budget;
        department.Budget = newBudget;
        department.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Departments", departmentId, AuditAction.Update, "System", "System User", 
            description: $"Updated department budget from {oldBudget:C} to {newBudget:C}");

        return true;
    }

    public async Task<bool> ActivateDepartmentAsync(int departmentId)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null) return false;

        department.IsActive = true;
        department.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Departments", departmentId, AuditAction.Update, "System", "System User", 
            description: $"Activated department: {department.Name}");

        return true;
    }

    public async Task<bool> DeactivateDepartmentAsync(int departmentId)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null) return false;

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Departments", departmentId, AuditAction.Update, "System", "System User", 
            description: $"Deactivated department: {department.Name}");

        return true;
    }

    #endregion

    #region Employee Operations

    public async Task<IEnumerable<Employee>> GetDepartmentEmployeesAsync(int departmentId)
    {
        _logger.LogInformation("Retrieving employees for department: {DepartmentId}", departmentId);
        return await _employeeService.GetEmployeesByDepartmentAsync(departmentId);
    }

    public async Task<bool> TransferEmployeeToDepartmentAsync(int employeeId, int newDepartmentId)
    {
        _logger.LogInformation("Transferring employee {EmployeeId} to department {DepartmentId}", employeeId, newDepartmentId);
        return await _employeeService.TransferEmployeeToDepartmentAsync(employeeId, newDepartmentId);
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        var employees = await GetDepartmentEmployeesAsync(departmentId);
        return employees.Count();
    }

    #endregion

    #region Project Operations

    public async Task<IEnumerable<Project>> GetDepartmentProjectsAsync(int departmentId)
    {
        _logger.LogInformation("Retrieving projects for department: {DepartmentId}", departmentId);
        return await _projectService.GetProjectsByDepartmentAsync(departmentId);
    }

    public async Task<int> GetActiveProjectCountAsync(int departmentId)
    {
        var projects = await GetDepartmentProjectsAsync(departmentId);
        return projects.Count(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Planning);
    }

    #endregion

    #region Statistics and Reports

    public async Task<decimal> GetDepartmentTotalSalariesAsync(int departmentId)
    {
        var employees = await GetDepartmentEmployeesAsync(departmentId);
        return employees.Sum(e => e.Salary);
    }

    public async Task<decimal> GetDepartmentBudgetUtilizationAsync(int departmentId)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null || department.Budget <= 0) return 0;

        var totalSalaries = await GetDepartmentTotalSalariesAsync(departmentId);
        var projects = await GetDepartmentProjectsAsync(departmentId);
        var totalProjectCosts = projects.Sum(p => p.ActualCost);

        var totalSpent = totalSalaries + totalProjectCosts;
        return (totalSpent / department.Budget) * 100;
    }

    public async Task<decimal> GetAverageSalaryAsync(int departmentId)
    {
        return await _employeeService.GetAverageSalaryByDepartmentAsync(departmentId);
    }

    public async Task<Dictionary<string, object>> GetDepartmentStatisticsAsync(int departmentId)
    {
        var department = await GetDepartmentByIdAsync(departmentId);
        if (department == null)
        {
            return new Dictionary<string, object>();
        }

        var employeeCount = await GetEmployeeCountAsync(departmentId);
        var activeProjectCount = await GetActiveProjectCountAsync(departmentId);
        var totalSalaries = await GetDepartmentTotalSalariesAsync(departmentId);
        var averageSalary = await GetAverageSalaryAsync(departmentId);
        var budgetUtilization = await GetDepartmentBudgetUtilizationAsync(departmentId);

        return new Dictionary<string, object>
        {
            ["DepartmentName"] = department.Name,
            ["EmployeeCount"] = employeeCount,
            ["ActiveProjectCount"] = activeProjectCount,
            ["TotalSalaries"] = totalSalaries,
            ["AverageSalary"] = averageSalary,
            ["Budget"] = department.Budget,
            ["BudgetUtilization"] = budgetUtilization,
            ["IsActive"] = department.IsActive,
            ["CreatedDate"] = department.CreatedAt,
            ["LastUpdated"] = department.UpdatedAt
        };
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByBudgetRangeAsync(decimal minBudget, decimal maxBudget)
    {
        await Task.Delay(50);
        return _departments.Where(d => d.Budget >= minBudget && d.Budget <= maxBudget);
    }

    #endregion

    #region Validation

    public async Task<bool> IsDepartmentCodeUniqueAsync(string code, int? excludeDepartmentId = null)
    {
        await Task.Delay(10);
        return !_departments.Any(d => d.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && 
                                     (!excludeDepartmentId.HasValue || d.Id != excludeDepartmentId.Value));
    }

    public async Task<bool> CanDeleteDepartmentAsync(int departmentId)
    {
        await Task.Delay(10);
        
        // Check if department has employees
        var employeeCount = await GetEmployeeCountAsync(departmentId);
        if (employeeCount > 0)
        {
            return false;
        }

        // Check if department has active projects
        var activeProjectCount = await GetActiveProjectCountAsync(departmentId);
        if (activeProjectCount > 0)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ValidateDepartmentDataAsync(Department department)
    {
        await Task.Delay(10);
        
        if (string.IsNullOrWhiteSpace(department.Name) || 
            string.IsNullOrWhiteSpace(department.Code))
        {
            return false;
        }

        if (!await IsDepartmentCodeUniqueAsync(department.Code, department.Id > 0 ? department.Id : null))
        {
            return false;
        }

        if (department.Budget < 0)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods

    private void InitializeSampleData()
    {
        var sampleDepartments = new[]
        {
            new Department { Id = _nextId++, Name = "Information Technology", Code = "IT", Description = "Technology and software development", Budget = 500000, ManagerId = 2, IsActive = true },
            new Department { Id = _nextId++, Name = "Human Resources", Code = "HR", Description = "Employee management and development", Budget = 200000, ManagerId = 5, IsActive = true },
            new Department { Id = _nextId++, Name = "Design & UX", Code = "UX", Description = "User experience and visual design", Budget = 150000, ManagerId = 3, IsActive = true },
            new Department { Id = _nextId++, Name = "Marketing", Code = "MKT", Description = "Marketing and customer outreach", Budget = 300000, ManagerId = null, IsActive = true },
            new Department { Id = _nextId++, Name = "Research & Development", Code = "RND", Description = "Innovation and product research", Budget = 400000, ManagerId = null, IsActive = false }
        };

        _departments.AddRange(sampleDepartments);
        _logger.LogInformation("Initialized with {Count} sample departments", sampleDepartments.Length);
    }

    #endregion
} 