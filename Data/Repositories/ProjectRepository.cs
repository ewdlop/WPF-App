using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context, ILogger<ProjectRepository> logger) 
        : base(context, logger)
    {
    }

    #region Project-specific queries

    public async Task<IEnumerable<Project>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet.Where(p => p.DepartmentId == departmentId)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status)
    {
        return await _dbSet.Where(p => p.Status == status)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByPriorityAsync(ProjectPriority priority)
    {
        return await _dbSet.Where(p => p.Priority == priority)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByManagerAsync(int managerId)
    {
        return await _dbSet.Where(p => p.ProjectManagerId == managerId)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _dbSet.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) ||
                p.Code.ToLower().Contains(lowerSearchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm)))
            .Include(p => p.Department)
            .Include(p => p.ProjectManager)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetOverdueAsync()
    {
        var currentDate = DateTime.UtcNow;
        return await _dbSet.Where(p => p.EndDate < currentDate && 
                                      p.Status != ProjectStatus.Completed && 
                                      p.Status != ProjectStatus.Cancelled)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.EndDate)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetActiveAsync()
    {
        return await _dbSet.Where(p => p.Status == ProjectStatus.InProgress)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetCompletedAsync()
    {
        return await _dbSet.Where(p => p.Status == ProjectStatus.Completed)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderByDescending(p => p.UpdatedAt)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetMostCriticalAsync(int count = 5)
    {
        return await _dbSet.Where(p => p.Status == ProjectStatus.InProgress)
                          .Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderByDescending(p => p.Priority)
                          .ThenBy(p => p.EndDate)
                          .Take(count)
                          .ToListAsync();
    }

    #endregion

    #region Project assignments

    public async Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId)
    {
        return await _context.ProjectAssignments
                            .Where(pa => pa.ProjectId == projectId)
                            .Include(pa => pa.Employee)
                            .Include(pa => pa.Project)
                            .OrderBy(pa => pa.Employee.LastName)
                            .ToListAsync();
    }

    public async Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int employeeId)
    {
        return await _context.ProjectAssignments
                            .Include(pa => pa.Employee)
                            .Include(pa => pa.Project)
                            .FirstOrDefaultAsync(pa => pa.ProjectId == projectId && pa.EmployeeId == employeeId);
    }

    public async Task<bool> AssignEmployeeAsync(int projectId, int employeeId, string role, decimal? hourlyRate = null)
    {
        var existingAssignment = await GetAssignmentAsync(projectId, employeeId);
        if (existingAssignment != null)
        {
            return false; // Already assigned
        }

        var assignment = new ProjectAssignment
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            Role = role,
            HourlyRate = hourlyRate,
            AssignedDate = DateTime.UtcNow,
            IsActive = true
        };

        await _context.ProjectAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnassignEmployeeAsync(int projectId, int employeeId)
    {
        var assignment = await GetAssignmentAsync(projectId, employeeId);
        if (assignment == null)
        {
            return false;
        }

        _context.ProjectAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAssignmentAsync(ProjectAssignment assignment)
    {
        _context.ProjectAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Project milestones

    public async Task<IEnumerable<ProjectMilestone>> GetProjectMilestonesAsync(int projectId)
    {
        return await _context.ProjectMilestones
                            .Where(pm => pm.ProjectId == projectId)
                            .OrderBy(pm => pm.DueDate)
                            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectMilestone>> GetUpcomingMilestonesAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(days);
        
        return await _context.ProjectMilestones
                            .Where(pm => !pm.IsCompleted && pm.DueDate >= startDate && pm.DueDate <= endDate)
                            .Include(pm => pm.Project)
                            .OrderBy(pm => pm.DueDate)
                            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectMilestone>> GetOverdueMilestonesAsync()
    {
        var currentDate = DateTime.UtcNow;
        return await _context.ProjectMilestones
                            .Where(pm => !pm.IsCompleted && pm.DueDate < currentDate)
                            .Include(pm => pm.Project)
                            .OrderBy(pm => pm.DueDate)
                            .ToListAsync();
    }

    public async Task<ProjectMilestone> CreateMilestoneAsync(ProjectMilestone milestone)
    {
        await _context.ProjectMilestones.AddAsync(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<ProjectMilestone> UpdateMilestoneAsync(ProjectMilestone milestone)
    {
        _context.ProjectMilestones.Update(milestone);
        await _context.SaveChangesAsync();
        return milestone;
    }

    public async Task<bool> DeleteMilestoneAsync(int milestoneId)
    {
        var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
        if (milestone == null)
        {
            return false;
        }

        _context.ProjectMilestones.Remove(milestone);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteMilestoneAsync(int milestoneId)
    {
        var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
        if (milestone == null)
        {
            return false;
        }

        milestone.IsCompleted = true;
        milestone.CompletedDate = DateTime.UtcNow;
        milestone.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Business operations

    public async Task<bool> UpdateStatusAsync(int projectId, ProjectStatus status)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null) return false;

        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProgressAsync(int projectId, decimal progressPercentage)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null) return false;

        project.ProgressPercentage = Math.Max(0, Math.Min(100, Convert.ToInt32(progressPercentage)));
        project.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanDeleteAsync(int projectId)
    {
        var hasAssignments = await _context.ProjectAssignments.AnyAsync(pa => pa.ProjectId == projectId);
        var hasMilestones = await _context.ProjectMilestones.AnyAsync(pm => pm.ProjectId == projectId);
        
        return !hasAssignments && !hasMilestones;
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _dbSet.CountAsync(p => p.Status == ProjectStatus.InProgress);
    }

    public async Task<int> GetCompletedCountAsync()
    {
        return await _dbSet.CountAsync(p => p.Status == ProjectStatus.Completed);
    }

    public async Task<decimal> GetTotalBudgetAsync()
    {
        return await _dbSet.SumAsync(p => p.Budget);
    }

    public async Task<decimal> GetBudgetUtilizationAsync()
    {
        var totalBudget = await GetTotalBudgetAsync();
        if (totalBudget == 0) return 0;
        
        var totalActualCost = await _dbSet.SumAsync(p => p.ActualCost);
        return (totalActualCost / totalBudget) * 100;
    }

    public async Task<Dictionary<ProjectStatus, int>> GetStatusDistributionAsync()
    {
        var distribution = await _dbSet.GroupBy(p => p.Status)
                                     .Select(g => new { Status = g.Key, Count = g.Count() })
                                     .ToListAsync();
        
        return distribution.ToDictionary(x => x.Status, x => x.Count);
    }

    #endregion

    #region Validation

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeProjectId = null)
    {
        var query = _dbSet.Where(p => p.Code.ToLower() == code.ToLower());
        
        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }
        
        return !await query.AnyAsync();
    }

    #endregion

    #region Advanced queries

    public async Task<Project?> GetByCodeAsync(string code)
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .Include(p => p.ProjectAssignments)
                          .ThenInclude(a => a.Employee)
                          .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());
    }

    public async Task<IEnumerable<Project>> GetWithAssignmentsAsync()
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .Include(p => p.ProjectAssignments)
                          .ThenInclude(a => a.Employee)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetWithMilestonesAsync()
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .Include(p => p.Milestones)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    public async Task<Project?> GetFullProjectAsync(int projectId)
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .Include(p => p.ProjectAssignments)
                          .ThenInclude(a => a.Employee)
                          .Include(p => p.Milestones)
                          .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    #endregion

    #region Override base methods for includes

    public override async Task<Project?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _dbSet.Include(p => p.Department)
                          .Include(p => p.ProjectManager)
                          .OrderBy(p => p.Name)
                          .ToListAsync();
    }

    #endregion
} 