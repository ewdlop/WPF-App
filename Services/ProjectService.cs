using Microsoft.Extensions.Logging;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.Services;

public class ProjectService : IProjectService
{
    private readonly ILogger<ProjectService> _logger;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // In-memory storage for demonstration
    private readonly List<Project> _projects = new();
    private readonly List<ProjectAssignment> _assignments = new();
    private readonly List<ProjectMilestone> _milestones = new();
    private int _nextProjectId = 1;
    private int _nextAssignmentId = 1;
    private int _nextMilestoneId = 1;

    public ProjectService(ILogger<ProjectService> logger, IAuditService auditService, INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        InitializeSampleData();
    }

    #region CRUD Operations

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        _logger.LogInformation("Retrieving all projects");
        await Task.Delay(100);
        return _projects.ToList();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving project with ID: {ProjectId}", id);
        await Task.Delay(50);
        return _projects.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        _logger.LogInformation("Creating new project: {ProjectName}", project.Name);
        
        if (!await ValidateProjectDataAsync(project))
        {
            throw new ArgumentException("Invalid project data");
        }

        project.Id = _nextProjectId++;
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        project.CreatedBy = "System";
        project.UpdatedBy = "System";

        _projects.Add(project);

        await _auditService.LogActionAsync("Projects", project.Id, AuditAction.Create, "System", "System User", 
            description: $"Created project: {project.Name}");

        await _notificationService.NotifyProjectCreatedAsync(project.Id);

        _logger.LogInformation("Project created successfully: {ProjectId}", project.Id);
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        _logger.LogInformation("Updating project: {ProjectId}", project.Id);
        
        var existingProject = await GetProjectByIdAsync(project.Id);
        if (existingProject == null)
        {
            throw new ArgumentException($"Project with ID {project.Id} not found");
        }

        var oldStatus = existingProject.Status;
        
        // Update fields
        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.Status = project.Status;
        existingProject.Priority = project.Priority;
        existingProject.Budget = project.Budget;
        existingProject.ActualCost = project.ActualCost;
        existingProject.StartDate = project.StartDate;
        existingProject.EndDate = project.EndDate;
        existingProject.EstimatedEndDate = project.EstimatedEndDate;
        existingProject.ProjectManagerId = project.ProjectManagerId;
        existingProject.DepartmentId = project.DepartmentId;
        existingProject.ProgressPercentage = project.ProgressPercentage;
        existingProject.UpdatedAt = DateTime.UtcNow;

        await _auditService.LogActionAsync("Projects", project.Id, AuditAction.Update, "System", "System User", 
            description: $"Updated project: {existingProject.Name}");

        // Notify if status changed
        if (oldStatus != project.Status)
        {
            await _notificationService.NotifyProjectStatusChangedAsync(project.Id, oldStatus.ToString(), project.Status.ToString());
        }

        return existingProject;
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        if (!await CanDeleteProjectAsync(id))
        {
            return false;
        }

        var project = await GetProjectByIdAsync(id);
        if (project == null) return false;

        _projects.Remove(project);

        // Remove related assignments and milestones
        var projectAssignments = _assignments.Where(a => a.ProjectId == id).ToList();
        var projectMilestones = _milestones.Where(m => m.ProjectId == id).ToList();

        foreach (var assignment in projectAssignments)
            _assignments.Remove(assignment);
        
        foreach (var milestone in projectMilestones)
            _milestones.Remove(milestone);

        await _auditService.LogActionAsync("Projects", id, AuditAction.Delete, "System", "System User", 
            description: $"Deleted project: {project.Name}");

        return true;
    }

    #endregion

    #region Search and Filter Operations

    public async Task<IEnumerable<Project>> SearchProjectsAsync(string searchTerm)
    {
        await Task.Delay(100);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllProjectsAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        return _projects.Where(p => 
            p.Name.ToLowerInvariant().Contains(lowerSearchTerm) ||
            p.Description?.ToLowerInvariant().Contains(lowerSearchTerm) == true ||
            p.Code.ToLowerInvariant().Contains(lowerSearchTerm));
    }

    public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(ProjectStatus status)
    {
        await Task.Delay(50);
        return _projects.Where(p => p.Status == status);
    }

    public async Task<IEnumerable<Project>> GetProjectsByPriorityAsync(ProjectPriority priority)
    {
        await Task.Delay(50);
        return _projects.Where(p => p.Priority == priority);
    }

    public async Task<IEnumerable<Project>> GetProjectsByDepartmentAsync(int departmentId)
    {
        await Task.Delay(50);
        return _projects.Where(p => p.DepartmentId == departmentId);
    }

    public async Task<IEnumerable<Project>> GetOverdueProjectsAsync()
    {
        await Task.Delay(50);
        var today = DateTime.UtcNow.Date;
        return _projects.Where(p => p.EndDate.HasValue && p.EndDate.Value.Date < today && 
                                   p.Status != ProjectStatus.Completed && p.Status != ProjectStatus.Cancelled);
    }

    public async Task<IEnumerable<Project>> GetProjectsByManagerAsync(int managerId)
    {
        await Task.Delay(50);
        return _projects.Where(p => p.ProjectManagerId == managerId);
    }

    #endregion

    #region Project Management Operations

    public async Task<bool> UpdateProjectStatusAsync(int projectId, ProjectStatus newStatus)
    {
        var project = await GetProjectByIdAsync(projectId);
        if (project == null) return false;

        var oldStatus = project.Status;
        project.Status = newStatus;
        project.UpdatedAt = DateTime.UtcNow;

        if (newStatus == ProjectStatus.Completed)
        {
            project.ProgressPercentage = 100;
        }

        await _auditService.LogActionAsync("Projects", projectId, AuditAction.Update, "System", "System User", 
            description: $"Status changed from {oldStatus} to {newStatus}");

        await _notificationService.NotifyProjectStatusChangedAsync(projectId, oldStatus.ToString(), newStatus.ToString());

        return true;
    }

    public async Task<bool> UpdateProjectProgressAsync(int projectId, int progressPercentage)
    {
        var project = await GetProjectByIdAsync(projectId);
        if (project == null || progressPercentage < 0 || progressPercentage > 100) return false;

        project.ProgressPercentage = progressPercentage;
        project.UpdatedAt = DateTime.UtcNow;

        if (progressPercentage == 100 && project.Status != ProjectStatus.Completed)
        {
            await UpdateProjectStatusAsync(projectId, ProjectStatus.Completed);
        }

        return true;
    }

    public async Task<bool> SetProjectManagerAsync(int projectId, int managerId)
    {
        var project = await GetProjectByIdAsync(projectId);
        if (project == null) return false;

        project.ProjectManagerId = managerId;
        project.UpdatedAt = DateTime.UtcNow;

        return true;
    }

    public async Task<bool> UpdateProjectBudgetAsync(int projectId, decimal newBudget)
    {
        var project = await GetProjectByIdAsync(projectId);
        if (project == null || newBudget < 0) return false;

        project.Budget = newBudget;
        project.UpdatedAt = DateTime.UtcNow;

        // Check if budget is exceeded
        if (project.ActualCost > newBudget)
        {
            await _notificationService.NotifyBudgetExceededAsync(projectId, newBudget, project.ActualCost);
        }

        return true;
    }

    public async Task<bool> UpdateProjectDatesAsync(int projectId, DateTime? startDate, DateTime? endDate, DateTime? estimatedEndDate)
    {
        var project = await GetProjectByIdAsync(projectId);
        if (project == null) return false;

        if (startDate.HasValue)
            project.StartDate = startDate.Value;
        
        project.EndDate = endDate;
        
        if (estimatedEndDate.HasValue)
            project.EstimatedEndDate = estimatedEndDate.Value;
            
        project.UpdatedAt = DateTime.UtcNow;

        return true;
    }

    #endregion

    #region Team Assignment Operations

    public async Task<ProjectAssignment> AssignEmployeeToProjectAsync(int projectId, int employeeId, string role, int allocationPercentage = 100)
    {
        var assignment = new ProjectAssignment
        {
            Id = _nextAssignmentId++,
            ProjectId = projectId,
            EmployeeId = employeeId,
            Role = role,
            AllocationPercentage = allocationPercentage,
            AssignedDate = DateTime.UtcNow,
            IsActive = true
        };

        _assignments.Add(assignment);

        await _auditService.LogActionAsync("ProjectAssignments", assignment.Id, AuditAction.Create, "System", "System User", 
            description: $"Assigned employee {employeeId} to project {projectId} as {role}");

        return assignment;
    }

    public async Task<bool> UnassignEmployeeFromProjectAsync(int assignmentId)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) return false;

        assignment.IsActive = false;
        assignment.UnassignedDate = DateTime.UtcNow;

        await _auditService.LogActionAsync("ProjectAssignments", assignmentId, AuditAction.Update, "System", "System User", 
            description: $"Unassigned employee from project");

        return true;
    }

    public async Task<bool> UpdateEmployeeRoleAsync(int assignmentId, string newRole)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) return false;

        assignment.Role = newRole;
        return true;
    }

    public async Task<bool> UpdateEmployeeAllocationAsync(int assignmentId, int allocationPercentage)
    {
        if (allocationPercentage < 0 || allocationPercentage > 100) return false;

        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) return false;

        assignment.AllocationPercentage = allocationPercentage;
        return true;
    }

    public async Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsAsync(int projectId)
    {
        await Task.Delay(30);
        return _assignments.Where(a => a.ProjectId == projectId && a.IsActive);
    }

    public async Task<IEnumerable<ProjectAssignment>> GetEmployeeAssignmentsAsync(int employeeId)
    {
        await Task.Delay(30);
        return _assignments.Where(a => a.EmployeeId == employeeId && a.IsActive);
    }

    #endregion

    #region Milestone Operations

    public async Task<ProjectMilestone> CreateMilestoneAsync(ProjectMilestone milestone)
    {
        milestone.Id = _nextMilestoneId++;
        _milestones.Add(milestone);

        await _auditService.LogActionAsync("ProjectMilestones", milestone.Id, AuditAction.Create, "System", "System User", 
            description: $"Created milestone: {milestone.Name}");

        return milestone;
    }

    public async Task<ProjectMilestone> UpdateMilestoneAsync(ProjectMilestone milestone)
    {
        var existingMilestone = _milestones.FirstOrDefault(m => m.Id == milestone.Id);
        if (existingMilestone == null)
        {
            throw new ArgumentException($"Milestone with ID {milestone.Id} not found");
        }

        existingMilestone.Name = milestone.Name;
        existingMilestone.Description = milestone.Description;
        existingMilestone.DueDate = milestone.DueDate;

        return existingMilestone;
    }

    public async Task<bool> DeleteMilestoneAsync(int milestoneId)
    {
        var milestone = _milestones.FirstOrDefault(m => m.Id == milestoneId);
        if (milestone == null) return false;

        _milestones.Remove(milestone);
        return true;
    }

    public async Task<bool> CompleteMilestoneAsync(int milestoneId)
    {
        var milestone = _milestones.FirstOrDefault(m => m.Id == milestoneId);
        if (milestone == null) return false;

        milestone.IsCompleted = true;
        milestone.CompletedDate = DateTime.UtcNow;

        await _notificationService.NotifyMilestoneCompletedAsync(milestoneId);

        return true;
    }

    public async Task<IEnumerable<ProjectMilestone>> GetProjectMilestonesAsync(int projectId)
    {
        await Task.Delay(30);
        return _milestones.Where(m => m.ProjectId == projectId).OrderBy(m => m.DueDate);
    }

    public async Task<IEnumerable<ProjectMilestone>> GetOverdueMilestonesAsync()
    {
        await Task.Delay(30);
        var today = DateTime.UtcNow.Date;
        return _milestones.Where(m => !m.IsCompleted && m.DueDate.Date < today);
    }

    public async Task<IEnumerable<ProjectMilestone>> GetUpcomingMilestonesAsync(int days = 7)
    {
        await Task.Delay(30);
        var targetDate = DateTime.UtcNow.AddDays(days).Date;
        return _milestones.Where(m => !m.IsCompleted && m.DueDate.Date <= targetDate);
    }

    #endregion

    #region Statistics and Reports

    public async Task<int> GetTotalProjectCountAsync()
    {
        await Task.Delay(10);
        return _projects.Count;
    }

    public async Task<int> GetActiveProjectCountAsync()
    {
        await Task.Delay(10);
        return _projects.Count(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Planning);
    }

    public async Task<int> GetCompletedProjectCountAsync()
    {
        await Task.Delay(10);
        return _projects.Count(p => p.Status == ProjectStatus.Completed);
    }

    public async Task<decimal> GetTotalBudgetAllocationAsync()
    {
        await Task.Delay(10);
        return _projects.Sum(p => p.Budget);
    }

    public async Task<decimal> GetBudgetUtilizationAsync()
    {
        await Task.Delay(10);
        var totalBudget = await GetTotalBudgetAllocationAsync();
        var totalSpent = _projects.Sum(p => p.ActualCost);
        return totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;
    }

    public async Task<IEnumerable<Project>> GetMostCriticalProjectsAsync(int count = 10)
    {
        await Task.Delay(50);
        return _projects.Where(p => p.Status == ProjectStatus.InProgress)
                       .OrderByDescending(p => p.Priority)
                       .ThenBy(p => p.EndDate)
                       .Take(count);
    }

    public async Task<Dictionary<ProjectStatus, int>> GetProjectStatusDistributionAsync()
    {
        await Task.Delay(30);
        return _projects.GroupBy(p => p.Status)
                       .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<ProjectPriority, int>> GetProjectPriorityDistributionAsync()
    {
        await Task.Delay(30);
        return _projects.GroupBy(p => p.Priority)
                       .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion

    #region Validation and Business Rules

    public async Task<bool> CanDeleteProjectAsync(int projectId)
    {
        await Task.Delay(10);
        var project = await GetProjectByIdAsync(projectId);
        if (project == null) return false;

        // Can't delete if project is in progress or has active assignments
        if (project.Status == ProjectStatus.InProgress)
            return false;

        var hasActiveAssignments = _assignments.Any(a => a.ProjectId == projectId && a.IsActive);
        return !hasActiveAssignments;
    }

    public async Task<bool> ValidateProjectDataAsync(Project project)
    {
        await Task.Delay(10);
        
        if (string.IsNullOrWhiteSpace(project.Name) || 
            string.IsNullOrWhiteSpace(project.Code))
        {
            return false;
        }

        if (!await IsProjectCodeUniqueAsync(project.Code, project.Id > 0 ? project.Id : null))
        {
            return false;
        }

        if (project.Budget < 0 || project.DepartmentId <= 0)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> IsProjectCodeUniqueAsync(string code, int? excludeProjectId = null)
    {
        await Task.Delay(10);
        return !_projects.Any(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && 
                                  (!excludeProjectId.HasValue || p.Id != excludeProjectId.Value));
    }

    #endregion

    #region Private Methods

    private void InitializeSampleData()
    {
        var sampleProjects = new[]
        {
            new Project { Id = _nextProjectId++, Name = "Customer Portal Redesign", Code = "CPR2024", Description = "Redesigning the customer portal for better UX", Status = ProjectStatus.InProgress, Priority = ProjectPriority.High, Budget = 150000, ActualCost = 75000, DepartmentId = 1, ProjectManagerId = 2, ProgressPercentage = 60, StartDate = DateTime.UtcNow.AddDays(-60), EndDate = DateTime.UtcNow.AddDays(30), EstimatedEndDate = DateTime.UtcNow.AddDays(30) },
            new Project { Id = _nextProjectId++, Name = "Mobile App Development", Code = "MAD2024", Description = "Developing a native mobile application", Status = ProjectStatus.Planning, Priority = ProjectPriority.Medium, Budget = 200000, ActualCost = 25000, DepartmentId = 1, ProjectManagerId = 4, ProgressPercentage = 15, StartDate = DateTime.UtcNow.AddDays(15), EndDate = DateTime.UtcNow.AddDays(120), EstimatedEndDate = DateTime.UtcNow.AddDays(120) },
            new Project { Id = _nextProjectId++, Name = "Data Migration", Code = "DM2024", Description = "Migrating legacy data to new system", Status = ProjectStatus.Completed, Priority = ProjectPriority.Critical, Budget = 80000, ActualCost = 78000, DepartmentId = 1, ProjectManagerId = 2, ProgressPercentage = 100, StartDate = DateTime.UtcNow.AddDays(-120), EndDate = DateTime.UtcNow.AddDays(-10), EstimatedEndDate = DateTime.UtcNow.AddDays(-10) }
        };

        _projects.AddRange(sampleProjects);
        _logger.LogInformation("Initialized with {Count} sample projects", sampleProjects.Length);
    }

    #endregion
} 