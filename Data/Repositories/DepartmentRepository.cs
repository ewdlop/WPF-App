using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context, ILogger<DepartmentRepository> logger) 
        : base(context, logger)
    {
    }

    #region Department-specific queries

    public async Task<IEnumerable<Department>> GetActiveAsync()
    {
        return await _dbSet.Where(d => d.IsActive)
                          .Include(d => d.Manager)
                          .OrderBy(d => d.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetInactiveAsync()
    {
        return await _dbSet.Where(d => !d.IsActive)
                          .Include(d => d.Manager)
                          .OrderBy(d => d.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Department>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet.Where(d => 
                d.Name.ToLower().Contains(lowerSearchTerm) ||
                d.Code.ToLower().Contains(lowerSearchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerSearchTerm)))
            .Include(d => d.Manager)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    #endregion

    #region Department operations

    public async Task<IEnumerable<Employee>> GetDepartmentEmployeesAsync(int departmentId)
    {
        return await _context.Employees
                            .Where(e => e.DepartmentId == departmentId)
                            .Include(e => e.Manager)
                            .OrderBy(e => e.LastName)
                            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetDepartmentProjectsAsync(int departmentId)
    {
        return await _context.Projects
                            .Where(p => p.DepartmentId == departmentId)
                            .Include(p => p.ProjectManager)
                            .OrderBy(p => p.Name)
                            .ToListAsync();
    }

    public async Task<Dictionary<string, object>> GetDepartmentStatisticsAsync(int departmentId)
    {
        var employees = await GetDepartmentEmployeesAsync(departmentId);
        var projects = await GetDepartmentProjectsAsync(departmentId);
        var activeEmployees = employees.Where(e => e.IsActive).ToList();
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.InProgress).ToList();

        var stats = new Dictionary<string, object>
        {
            ["TotalEmployees"] = employees.Count(),
            ["ActiveEmployees"] = activeEmployees.Count,
            ["TotalProjects"] = projects.Count(),
            ["ActiveProjects"] = activeProjects.Count,
            ["AverageSalary"] = activeEmployees.Any() ? activeEmployees.Average(e => e.Salary) : 0m,
            ["TotalSalaryExpense"] = activeEmployees.Sum(e => e.Salary),
            ["ProjectBudget"] = projects.Sum(p => p.Budget),
            ["ProjectActualCost"] = projects.Sum(p => p.ActualCost)
        };

        return stats;
    }

    public async Task<decimal> GetBudgetUtilizationAsync(int departmentId)
    {
        var department = await GetByIdAsync(departmentId);
        if (department == null || department.Budget == 0) return 0;

        var employees = await GetDepartmentEmployeesAsync(departmentId);
        var projects = await GetDepartmentProjectsAsync(departmentId);
        
        var totalSalaryExpense = employees.Where(e => e.IsActive).Sum(e => e.Salary);
        var totalProjectCost = projects.Sum(p => p.ActualCost);
        var totalExpense = totalSalaryExpense + totalProjectCost;

        return (totalExpense / department.Budget) * 100;
    }

    #endregion

    #region Business operations

    public async Task<bool> ActivateAsync(int departmentId)
    {
        var department = await GetByIdAsync(departmentId);
        if (department == null) return false;

        department.IsActive = true;
        department.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int departmentId)
    {
        var department = await GetByIdAsync(departmentId);
        if (department == null) return false;

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanDeleteAsync(int departmentId)
    {
        var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId);
        var hasProjects = await _context.Projects.AnyAsync(p => p.DepartmentId == departmentId);
        
        return !hasEmployees && !hasProjects;
    }

    #endregion

    #region Validation

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeDepartmentId = null)
    {
        var query = _dbSet.Where(d => d.Code.ToLower() == code.ToLower());
        
        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(d => d.Id != excludeDepartmentId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeDepartmentId = null)
    {
        var query = _dbSet.Where(d => d.Name.ToLower() == name.ToLower());
        
        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(d => d.Id != excludeDepartmentId.Value);
        }
        
        return !await query.AnyAsync();
    }

    #endregion

    #region Advanced queries

    public async Task<Department?> GetByCodeAsync(string code)
    {
        return await _dbSet.Include(d => d.Manager)
                          .Include(d => d.Employees)
                          .Include(d => d.Projects)
                          .FirstOrDefaultAsync(d => d.Code.ToLower() == code.ToLower());
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _dbSet.Include(d => d.Manager)
                          .Include(d => d.Employees)
                          .Include(d => d.Projects)
                          .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Department>> GetWithEmployeesAsync()
    {
        return await _dbSet.Include(d => d.Manager)
                          .Include(d => d.Employees)
                          .Where(d => d.IsActive)
                          .OrderBy(d => d.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetWithProjectsAsync()
    {
        return await _dbSet.Include(d => d.Manager)
                          .Include(d => d.Projects)
                          .Where(d => d.IsActive)
                          .OrderBy(d => d.Name)
                          .ToListAsync();
    }

    public async Task<Department?> GetFullDepartmentAsync(int departmentId)
    {
        return await _dbSet.Include(d => d.Manager)
                          .Include(d => d.Employees)
                          .ThenInclude(e => e.Manager)
                          .Include(d => d.Projects)
                          .ThenInclude(p => p.ProjectManager)
                          .FirstOrDefaultAsync(d => d.Id == departmentId);
    }

    #endregion

    #region Statistics

    public async Task<decimal> GetTotalBudgetAsync()
    {
        return await _dbSet.Where(d => d.IsActive).SumAsync(d => d.Budget);
    }

    public async Task<int> GetTotalEmployeeCountAsync()
    {
        var departments = await _dbSet.Include(d => d.Employees).ToListAsync();
        return departments.SelectMany(d => d.Employees).Count();
    }

    public async Task<int> GetActiveEmployeeCountAsync()
    {
        var departments = await _dbSet.Include(d => d.Employees).ToListAsync();
        return departments.SelectMany(d => d.Employees).Count(e => e.IsActive);
    }

    #endregion

    #region Override base methods for includes

    public override async Task<Department?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(d => d.Manager)
                          .FirstOrDefaultAsync(d => d.Id == id);
    }

    public override async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _dbSet.Include(d => d.Manager)
                          .OrderBy(d => d.Name)
                          .ToListAsync();
    }

    #endregion
} 