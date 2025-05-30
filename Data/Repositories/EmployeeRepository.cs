using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context, ILogger<EmployeeRepository> logger) 
        : base(context, logger)
    {
    }

    #region Employee-specific queries

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet.Where(e => e.DepartmentId == departmentId)
                          .Include(e => e.Department)
                          .Include(e => e.Manager)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        return await _dbSet.Where(e => e.IsActive)
                          .Include(e => e.Department)
                          .Include(e => e.Manager)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetInactiveEmployeesAsync()
    {
        return await _dbSet.Where(e => !e.IsActive)
                          .Include(e => e.Department)
                          .Include(e => e.Manager)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetByManagerAsync(int managerId)
    {
        return await _dbSet.Where(e => e.ManagerId == managerId)
                          .Include(e => e.Department)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet.Where(e => 
                e.FirstName.ToLower().Contains(lowerSearchTerm) ||
                e.LastName.ToLower().Contains(lowerSearchTerm) ||
                e.Email.ToLower().Contains(lowerSearchTerm) ||
                e.Position.ToLower().Contains(lowerSearchTerm) ||
                e.EmployeeNumber.ToLower().Contains(lowerSearchTerm))
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    #endregion

    #region Validation methods

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeEmployeeId = null)
    {
        var query = _dbSet.Where(e => e.Email.ToLower() == email.ToLower());
        
        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEmployeeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmployeeNumberUniqueAsync(string employeeNumber, int? excludeEmployeeId = null)
    {
        var query = _dbSet.Where(e => e.EmployeeNumber.ToLower() == employeeNumber.ToLower());
        
        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEmployeeId.Value);
        }
        
        return !await query.AnyAsync();
    }

    #endregion

    #region Business operations

    public async Task<bool> ActivateAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return false;

        employee.IsActive = true;
        employee.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return false;

        employee.IsActive = false;
        employee.TerminationDate = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferToDepartmentAsync(int employeeId, int newDepartmentId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return false;

        employee.DepartmentId = newDepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateSalaryAsync(int employeeId, decimal newSalary)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return false;

        employee.Salary = newSalary;
        employee.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PromoteAsync(int employeeId, string newPosition, decimal newSalary)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return false;

        employee.Position = newPosition;
        employee.Salary = newSalary;
        employee.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _dbSet.CountAsync(e => e.IsActive);
    }

    public async Task<decimal> GetAverageSalaryAsync()
    {
        var activeEmployees = await _dbSet.Where(e => e.IsActive).ToListAsync();
        return activeEmployees.Any() ? activeEmployees.Average(e => e.Salary) : 0;
    }

    public async Task<decimal> GetAverageSalaryByDepartmentAsync(int departmentId)
    {
        var departmentEmployees = await _dbSet.Where(e => e.DepartmentId == departmentId && e.IsActive).ToListAsync();
        return departmentEmployees.Any() ? departmentEmployees.Average(e => e.Salary) : 0;
    }

    public async Task<IEnumerable<Employee>> GetTopPerformersAsync(int count = 10)
    {
        // For now, order by salary as a proxy for performance
        return await _dbSet.Where(e => e.IsActive)
                          .Include(e => e.Department)
                          .OrderByDescending(e => e.Salary)
                          .Take(count)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetRecentHiresAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _dbSet.Where(e => e.HireDate >= cutoffDate)
                          .Include(e => e.Department)
                          .OrderByDescending(e => e.HireDate)
                          .ToListAsync();
    }

    #endregion

    #region Advanced queries

    public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.Manager)
                          .Include(e => e.DirectReports)
                          .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.Manager)
                          .Include(e => e.DirectReports)
                          .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Employee>> GetWithProjectsAsync()
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.Manager)
                          .Include(e => e.ProjectAssignments)
                          .ThenInclude(pa => pa.Project)
                          .Where(e => e.IsActive)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Employee>> GetManagersAsync()
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.DirectReports)
                          .Where(e => e.IsActive && e.DirectReports.Any())
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    #endregion

    #region Override base methods for includes

    public override async Task<Employee?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.Manager)
                          .Include(e => e.DirectReports)
                          .FirstOrDefaultAsync(e => e.Id == id);
    }

    public override async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _dbSet.Include(e => e.Department)
                          .Include(e => e.Manager)
                          .OrderBy(e => e.LastName)
                          .ToListAsync();
    }

    #endregion
} 