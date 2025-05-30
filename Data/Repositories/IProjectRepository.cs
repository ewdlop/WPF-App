using WpfApp2.Models;

namespace WpfApp2.Data.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    // Project-specific queries
    Task<IEnumerable<Project>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status);
    Task<IEnumerable<Project>> GetByPriorityAsync(ProjectPriority priority);
    Task<IEnumerable<Project>> GetByManagerAsync(int managerId);
    Task<IEnumerable<Project>> SearchAsync(string searchTerm);
    Task<IEnumerable<Project>> GetOverdueAsync();
    Task<IEnumerable<Project>> GetActiveAsync();
    Task<IEnumerable<Project>> GetCompletedAsync();
    Task<IEnumerable<Project>> GetMostCriticalAsync(int count = 5);
    
    // Project assignments
    Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId);
    Task<ProjectAssignment?> GetAssignmentAsync(int projectId, int employeeId);
    Task<bool> AssignEmployeeAsync(int projectId, int employeeId, string role, decimal? hourlyRate = null);
    Task<bool> UnassignEmployeeAsync(int projectId, int employeeId);
    Task<bool> UpdateAssignmentAsync(ProjectAssignment assignment);
    
    // Project milestones
    Task<IEnumerable<ProjectMilestone>> GetProjectMilestonesAsync(int projectId);
    Task<IEnumerable<ProjectMilestone>> GetUpcomingMilestonesAsync(int days = 7);
    Task<IEnumerable<ProjectMilestone>> GetOverdueMilestonesAsync();
    Task<ProjectMilestone> CreateMilestoneAsync(ProjectMilestone milestone);
    Task<ProjectMilestone> UpdateMilestoneAsync(ProjectMilestone milestone);
    Task<bool> DeleteMilestoneAsync(int milestoneId);
    Task<bool> CompleteMilestoneAsync(int milestoneId);
    
    // Business operations
    Task<bool> UpdateStatusAsync(int projectId, ProjectStatus status);
    Task<bool> UpdateProgressAsync(int projectId, decimal progressPercentage);
    Task<bool> CanDeleteAsync(int projectId);
    
    // Statistics
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task<int> GetCompletedCountAsync();
    Task<decimal> GetTotalBudgetAsync();
    Task<decimal> GetBudgetUtilizationAsync();
    Task<Dictionary<ProjectStatus, int>> GetStatusDistributionAsync();
    
    // Validation
    Task<bool> IsCodeUniqueAsync(string code, int? excludeProjectId = null);
    
    // Advanced queries
    Task<Project?> GetByCodeAsync(string code);
    Task<IEnumerable<Project>> GetWithAssignmentsAsync();
    Task<IEnumerable<Project>> GetWithMilestonesAsync();
    Task<Project?> GetFullProjectAsync(int projectId); // Includes all related data
} 