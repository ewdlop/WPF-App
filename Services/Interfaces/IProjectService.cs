using WpfApp2.Models;

namespace WpfApp2.Services.Interfaces;

public interface IProjectService
{
    // CRUD Operations
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(int id);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(int id);

    // Search and Filter Operations
    Task<IEnumerable<Project>> SearchProjectsAsync(string searchTerm);
    Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status);
    Task<IEnumerable<Project>> GetProjectsByPriorityAsync(ProjectPriority priority);
    Task<IEnumerable<Project>> GetProjectsByDepartmentAsync(int departmentId);
    Task<IEnumerable<Project>> GetOverdueProjectsAsync();
    Task<IEnumerable<Project>> GetProjectsByManagerAsync(int managerId);

    // Project Management Operations
    Task<bool> UpdateProjectStatusAsync(int projectId, ProjectStatus newStatus);
    Task<bool> UpdateProjectProgressAsync(int projectId, int progressPercentage);
    Task<bool> SetProjectManagerAsync(int projectId, int managerId);
    Task<bool> UpdateProjectBudgetAsync(int projectId, decimal newBudget);
    Task<bool> UpdateProjectDatesAsync(int projectId, DateTime? startDate, DateTime? endDate, DateTime? estimatedEndDate);

    // Team Assignment Operations
    Task<ProjectAssignment> AssignEmployeeToProjectAsync(int projectId, int employeeId, string role, int allocationPercentage = 100);
    Task<bool> UnassignEmployeeFromProjectAsync(int assignmentId);
    Task<bool> UpdateEmployeeRoleAsync(int assignmentId, string newRole);
    Task<bool> UpdateEmployeeAllocationAsync(int assignmentId, int allocationPercentage);
    Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId);
    Task<IEnumerable<ProjectAssignment>> GetEmployeeAssignmentsAsync(int employeeId);

    // Milestone Operations
    Task<ProjectMilestone> CreateMilestoneAsync(ProjectMilestone milestone);
    Task<ProjectMilestone> UpdateMilestoneAsync(ProjectMilestone milestone);
    Task<bool> DeleteMilestoneAsync(int milestoneId);
    Task<bool> CompleteMilestoneAsync(int milestoneId);
    Task<IEnumerable<ProjectMilestone>> GetProjectMilestonesAsync(int projectId);
    Task<IEnumerable<ProjectMilestone>> GetOverdueMilestonesAsync();
    Task<IEnumerable<ProjectMilestone>> GetUpcomingMilestonesAsync(int days = 7);

    // Statistics and Reports
    Task<int> GetTotalProjectCountAsync();
    Task<int> GetActiveProjectCountAsync();
    Task<int> GetCompletedProjectCountAsync();
    Task<decimal> GetTotalBudgetAllocationAsync();
    Task<decimal> GetBudgetUtilizationAsync();
    Task<IEnumerable<Project>> GetMostCriticalProjectsAsync(int count = 10);
    Task<Dictionary<ProjectStatus, int>> GetProjectStatusDistributionAsync();
    Task<Dictionary<ProjectPriority, int>> GetProjectPriorityDistributionAsync();

    // Validation and Business Rules
    Task<bool> CanDeleteProjectAsync(int projectId);
    Task<bool> ValidateProjectDataAsync(Project project);
    Task<bool> IsProjectCodeUniqueAsync(string code, int? excludeProjectId = null);
} 