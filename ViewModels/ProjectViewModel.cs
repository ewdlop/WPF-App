using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class ProjectViewModel : BaseViewModel
{
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // Collections
    private readonly ObservableCollection<Project> _allProjects = new();
    private ICollectionView? _projectsView;

    // Properties
    private Project? _selectedProject;
    private Project _currentProject = new();
    private bool _isEditMode;
    private string _searchText = string.Empty;
    private ProjectStatus? _selectedStatusFilter;
    private ProjectPriority? _selectedPriorityFilter;
    private Department? _selectedDepartmentFilter;
    private string _selectedSortProperty = nameof(Project.Name);
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;

    // Selected project details
    private ProjectAssignment? _selectedAssignment;
    private ProjectMilestone? _selectedMilestone;
    private ProjectMilestone _currentMilestone = new();
    private bool _isMilestoneEditMode;

    // Statistics
    private int _totalProjects;
    private int _activeProjects;
    private int _completedProjects;
    private int _overdueProjects;
    private decimal _totalBudget;
    private decimal _budgetUtilization;

    public ProjectViewModel(ILogger<ProjectViewModel> logger, 
        IProjectService projectService, IEmployeeService employeeService,
        IDepartmentService departmentService, IAuditService auditService,
        INotificationService notificationService) 
        : base(logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Initialize collections
        Departments = new ObservableCollection<Department>();
        Employees = new ObservableCollection<Employee>();
        ProjectAssignments = new ObservableCollection<ProjectAssignment>();
        ProjectMilestones = new ObservableCollection<ProjectMilestone>();
        StatusOptions = new ObservableCollection<ProjectStatus>
        {
            ProjectStatus.Planning,
            ProjectStatus.InProgress,
            ProjectStatus.OnHold,
            ProjectStatus.Completed,
            ProjectStatus.Cancelled
        };
        PriorityOptions = new ObservableCollection<ProjectPriority>
        {
            ProjectPriority.Low,
            ProjectPriority.Medium,
            ProjectPriority.High,
            ProjectPriority.Critical
        };

        // Initialize commands
        LoadProjectsCommand = new AsyncRelayCommand(LoadProjectsAsync, () => !IsBusy);
        SearchCommand = new AsyncRelayCommand(SearchProjectsAsync, () => !IsBusy);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        AddProjectCommand = new RelayCommand(AddProject, () => !IsBusy);
        EditProjectCommand = new RelayCommand(EditProject, () => SelectedProject != null && !IsBusy);
        SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync, () => !IsBusy && !HasErrors);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditMode);
        DeleteProjectCommand = new AsyncRelayCommand(DeleteProjectAsync, () => SelectedProject != null && !IsBusy);
        RefreshCommand = new AsyncRelayCommand(RefreshDataAsync, () => !IsBusy);
        
        // Project detail commands
        LoadProjectDetailsCommand = new AsyncRelayCommand(LoadProjectDetailsAsync, () => SelectedProject != null && !IsBusy);
        AssignEmployeeCommand = new AsyncRelayCommand(AssignEmployeeAsync, () => SelectedProject != null && !IsBusy);
        UnassignEmployeeCommand = new AsyncRelayCommand(UnassignEmployeeAsync, () => SelectedAssignment != null && !IsBusy);
        UpdateProjectStatusCommand = new AsyncRelayCommand(UpdateProjectStatusAsync, () => SelectedProject != null && !IsBusy);
        UpdateProgressCommand = new AsyncRelayCommand(UpdateProgressAsync, () => SelectedProject != null && !IsBusy);
        
        // Milestone commands
        AddMilestoneCommand = new RelayCommand(AddMilestone, () => SelectedProject != null && !IsBusy);
        EditMilestoneCommand = new RelayCommand(EditMilestone, () => SelectedMilestone != null && !IsBusy);
        SaveMilestoneCommand = new AsyncRelayCommand(SaveMilestoneAsync, () => !IsBusy);
        DeleteMilestoneCommand = new AsyncRelayCommand(DeleteMilestoneAsync, () => SelectedMilestone != null && !IsBusy);
        CompleteMilestoneCommand = new AsyncRelayCommand(CompleteMilestoneAsync, () => SelectedMilestone != null && !IsBusy);

        _logger.LogInformation("ProjectViewModel initialized");
    }

    #region Properties

    public ObservableCollection<Department> Departments { get; }
    public ObservableCollection<Employee> Employees { get; }
    public ObservableCollection<ProjectAssignment> ProjectAssignments { get; }
    public ObservableCollection<ProjectMilestone> ProjectMilestones { get; }
    public ObservableCollection<ProjectStatus> StatusOptions { get; }
    public ObservableCollection<ProjectPriority> PriorityOptions { get; }

    public ICollectionView ProjectsView
    {
        get
        {
            if (_projectsView == null)
            {
                _projectsView = CollectionViewSource.GetDefaultView(_allProjects);
                _projectsView.Filter = ProjectFilter;
                _projectsView.SortDescriptions.Add(new SortDescription(_selectedSortProperty, _sortDirection));
            }
            return _projectsView;
        }
    }

    public Project? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                _ = LoadProjectDetailsAsync();
            }
        }
    }

    public Project CurrentProject
    {
        get => _currentProject;
        set => SetProperty(ref _currentProject, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _projectsView?.Refresh();
            }
        }
    }

    public ProjectStatus? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                _projectsView?.Refresh();
            }
        }
    }

    public ProjectPriority? SelectedPriorityFilter
    {
        get => _selectedPriorityFilter;
        set
        {
            if (SetProperty(ref _selectedPriorityFilter, value))
            {
                _projectsView?.Refresh();
            }
        }
    }

    public Department? SelectedDepartmentFilter
    {
        get => _selectedDepartmentFilter;
        set
        {
            if (SetProperty(ref _selectedDepartmentFilter, value))
            {
                _projectsView?.Refresh();
            }
        }
    }

    // Project details properties
    public ProjectAssignment? SelectedAssignment
    {
        get => _selectedAssignment;
        set => SetProperty(ref _selectedAssignment, value);
    }

    public ProjectMilestone? SelectedMilestone
    {
        get => _selectedMilestone;
        set => SetProperty(ref _selectedMilestone, value);
    }

    public ProjectMilestone CurrentMilestone
    {
        get => _currentMilestone;
        set => SetProperty(ref _currentMilestone, value);
    }

    public bool IsMilestoneEditMode
    {
        get => _isMilestoneEditMode;
        set => SetProperty(ref _isMilestoneEditMode, value);
    }

    #endregion

    #region Statistics Properties

    public int TotalProjects
    {
        get => _totalProjects;
        set => SetProperty(ref _totalProjects, value);
    }

    public int ActiveProjects
    {
        get => _activeProjects;
        set => SetProperty(ref _activeProjects, value);
    }

    public int CompletedProjects
    {
        get => _completedProjects;
        set => SetProperty(ref _completedProjects, value);
    }

    public int OverdueProjects
    {
        get => _overdueProjects;
        set => SetProperty(ref _overdueProjects, value);
    }

    public decimal TotalBudget
    {
        get => _totalBudget;
        set => SetProperty(ref _totalBudget, value);
    }

    public decimal BudgetUtilization
    {
        get => _budgetUtilization;
        set => SetProperty(ref _budgetUtilization, value);
    }

    #endregion

    #region Commands

    public ICommand LoadProjectsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand AddProjectCommand { get; }
    public ICommand EditProjectCommand { get; }
    public ICommand SaveProjectCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand RefreshCommand { get; }
    
    // Project detail commands
    public ICommand LoadProjectDetailsCommand { get; }
    public ICommand AssignEmployeeCommand { get; }
    public ICommand UnassignEmployeeCommand { get; }
    public ICommand UpdateProjectStatusCommand { get; }
    public ICommand UpdateProgressCommand { get; }
    
    // Milestone commands
    public ICommand AddMilestoneCommand { get; }
    public ICommand EditMilestoneCommand { get; }
    public ICommand SaveMilestoneCommand { get; }
    public ICommand DeleteMilestoneCommand { get; }
    public ICommand CompleteMilestoneCommand { get; }

    #endregion

    #region Initialization

    public override async Task InitializeAsync()
    {
        await LoadDepartmentsAsync();
        await LoadEmployeesAsync();
        await LoadProjectsAsync();
    }

    public override async Task LoadDataAsync()
    {
        await LoadProjectsAsync();
    }

    #endregion

    #region Data Loading

    private async Task LoadProjectsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var projects = await _projectService.GetAllProjectsAsync();
            
            _allProjects.Clear();
            foreach (var project in projects)
            {
                _allProjects.Add(project);
            }

            await LoadStatisticsAsync();
            
            _logger.LogInformation("Loaded {Count} projects", _allProjects.Count);
        }, "Loading projects...");
    }

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            
            Departments.Clear();
            Departments.Add(new Department { Id = 0, Name = "All Departments" }); // Filter option
            
            foreach (var dept in departments.Where(d => d.IsActive))
            {
                Departments.Add(dept);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load departments");
        }
    }

    private async Task LoadEmployeesAsync()
    {
        try
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            
            Employees.Clear();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load employees");
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            TotalProjects = await _projectService.GetTotalProjectCountAsync();
            ActiveProjects = await _projectService.GetActiveProjectCountAsync();
            CompletedProjects = await _projectService.GetCompletedProjectCountAsync();
            TotalBudget = await _projectService.GetTotalBudgetAllocationAsync();
            BudgetUtilization = await _projectService.GetBudgetUtilizationAsync();
            
            var overdueProjects = await _projectService.GetOverdueProjectsAsync();
            OverdueProjects = overdueProjects.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project statistics");
        }
    }

    private async Task LoadProjectDetailsAsync()
    {
        if (SelectedProject == null) return;

        await ExecuteAsync(async () =>
        {
            // Load project assignments
            var assignments = await _projectService.GetProjectAssignmentsAsync(SelectedProject.Id);
            ProjectAssignments.Clear();
            foreach (var assignment in assignments)
            {
                ProjectAssignments.Add(assignment);
            }

            // Load project milestones
            var milestones = await _projectService.GetProjectMilestonesAsync(SelectedProject.Id);
            ProjectMilestones.Clear();
            foreach (var milestone in milestones)
            {
                ProjectMilestones.Add(milestone);
            }
        }, "Loading project details...");
    }

    #endregion

    #region Search and Filter

    private async Task SearchProjectsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadProjectsAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            var searchResults = await _projectService.SearchProjectsAsync(SearchText);
            
            _allProjects.Clear();
            foreach (var project in searchResults)
            {
                _allProjects.Add(project);
            }
        }, "Searching projects...");
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectedStatusFilter = null;
        SelectedPriorityFilter = null;
        SelectedDepartmentFilter = null;
        _ = LoadProjectsAsync();
    }

    private bool ProjectFilter(object item)
    {
        if (item is not Project project)
            return false;

        // Status filter
        if (SelectedStatusFilter.HasValue && project.Status != SelectedStatusFilter.Value)
            return false;

        // Priority filter
        if (SelectedPriorityFilter.HasValue && project.Priority != SelectedPriorityFilter.Value)
            return false;

        // Department filter
        if (SelectedDepartmentFilter != null && SelectedDepartmentFilter.Id > 0 && 
            project.DepartmentId != SelectedDepartmentFilter.Id)
            return false;

        // Text search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            return project.Name.ToLowerInvariant().Contains(searchLower) ||
                   project.Code.ToLowerInvariant().Contains(searchLower) ||
                   (project.Description?.ToLowerInvariant().Contains(searchLower) ?? false);
        }

        return true;
    }

    #endregion

    #region CRUD Operations

    private void AddProject()
    {
        CurrentProject = new Project
        {
            StartDate = DateTime.UtcNow,
            Status = ProjectStatus.Planning,
            Priority = ProjectPriority.Medium,
            DepartmentId = Departments.FirstOrDefault(d => d.Id > 0)?.Id ?? 1,
            ProgressPercentage = 0
        };
        IsEditMode = true;
        SelectedProject = null;
    }

    private void EditProject()
    {
        if (SelectedProject != null)
        {
            // Create a copy for editing
            CurrentProject = new Project
            {
                Id = SelectedProject.Id,
                Name = SelectedProject.Name,
                Code = SelectedProject.Code,
                Description = SelectedProject.Description,
                Status = SelectedProject.Status,
                Priority = SelectedProject.Priority,
                Budget = SelectedProject.Budget,
                ActualCost = SelectedProject.ActualCost,
                StartDate = SelectedProject.StartDate,
                EndDate = SelectedProject.EndDate,
                EstimatedEndDate = SelectedProject.EstimatedEndDate,
                ProjectManagerId = SelectedProject.ProjectManagerId,
                DepartmentId = SelectedProject.DepartmentId,
                ProgressPercentage = SelectedProject.ProgressPercentage
            };
            IsEditMode = true;
        }
    }

    private async Task SaveProjectAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                if (CurrentProject.Id == 0)
                {
                    // Adding new project
                    var newProject = await _projectService.CreateProjectAsync(CurrentProject);
                    _allProjects.Add(newProject);
                    await _notificationService.ShowToastAsync("Success", $"Project '{newProject.Name}' has been created.");
                }
                else
                {
                    // Updating existing project
                    var updatedProject = await _projectService.UpdateProjectAsync(CurrentProject);
                    var index = _allProjects.ToList().FindIndex(p => p.Id == updatedProject.Id);
                    if (index >= 0)
                    {
                        _allProjects[index] = updatedProject;
                    }
                    await _notificationService.ShowToastAsync("Success", $"Project '{updatedProject.Name}' has been updated.");
                }

                IsEditMode = false;
                await LoadStatisticsAsync();
            }
            catch (ArgumentException ex)
            {
                await _notificationService.ShowToastAsync("Validation Error", ex.Message);
            }
        }, "Saving project...");
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentProject = new Project();
        ClearErrors();
    }

    private async Task DeleteProjectAsync()
    {
        if (SelectedProject == null) return;

        await ExecuteAsync(async () =>
        {
            var canDelete = await _projectService.CanDeleteProjectAsync(SelectedProject.Id);
            if (!canDelete)
            {
                await _notificationService.ShowToastAsync("Error", "Cannot delete project. It may have active assignments or be in progress.");
                return;
            }

            var result = await _projectService.DeleteProjectAsync(SelectedProject.Id);
            if (result)
            {
                _allProjects.Remove(SelectedProject);
                await _notificationService.ShowToastAsync("Success", $"Project '{SelectedProject.Name}' has been deleted.");
                SelectedProject = null;
                await LoadStatisticsAsync();
            }
        }, "Deleting project...");
    }

    #endregion

    #region Project Management Operations

    private async Task AssignEmployeeAsync()
    {
        if (SelectedProject == null) return;

        await ExecuteAsync(async () =>
        {
            // This would typically open a dialog to select employee and role
            _logger.LogInformation("Assigning employee to project {ProjectId}", SelectedProject.Id);
            await _notificationService.ShowToastAsync("Assign Employee", "Employee assignment dialog would open here.");
        }, "Assigning employee...");
    }

    private async Task UnassignEmployeeAsync()
    {
        if (SelectedAssignment == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _projectService.UnassignEmployeeFromProjectAsync(SelectedAssignment.Id);
            if (result)
            {
                ProjectAssignments.Remove(SelectedAssignment);
                await _notificationService.ShowToastAsync("Success", "Employee has been unassigned from the project.");
                SelectedAssignment = null;
            }
        }, "Unassigning employee...");
    }

    private async Task UpdateProjectStatusAsync()
    {
        if (SelectedProject == null) return;

        await ExecuteAsync(async () =>
        {
            // This would typically open a dialog to select new status
            _logger.LogInformation("Updating status for project {ProjectId}", SelectedProject.Id);
            await _notificationService.ShowToastAsync("Update Status", "Status update dialog would open here.");
        }, "Updating project status...");
    }

    private async Task UpdateProgressAsync()
    {
        if (SelectedProject == null) return;

        await ExecuteAsync(async () =>
        {
            // This would typically open a dialog to update progress
            _logger.LogInformation("Updating progress for project {ProjectId}", SelectedProject.Id);
            await _notificationService.ShowToastAsync("Update Progress", "Progress update dialog would open here.");
        }, "Updating project progress...");
    }

    #endregion

    #region Milestone Operations

    private void AddMilestone()
    {
        if (SelectedProject == null) return;

        CurrentMilestone = new ProjectMilestone
        {
            ProjectId = SelectedProject.Id,
            DueDate = DateTime.UtcNow.AddDays(30),
            IsCompleted = false
        };
        IsMilestoneEditMode = true;
        SelectedMilestone = null;
    }

    private void EditMilestone()
    {
        if (SelectedMilestone != null)
        {
            CurrentMilestone = new ProjectMilestone
            {
                Id = SelectedMilestone.Id,
                ProjectId = SelectedMilestone.ProjectId,
                Name = SelectedMilestone.Name,
                Description = SelectedMilestone.Description,
                DueDate = SelectedMilestone.DueDate,
                IsCompleted = SelectedMilestone.IsCompleted,
                CompletedDate = SelectedMilestone.CompletedDate
            };
            IsMilestoneEditMode = true;
        }
    }

    private async Task SaveMilestoneAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (CurrentMilestone.Id == 0)
            {
                // Adding new milestone
                var newMilestone = await _projectService.CreateMilestoneAsync(CurrentMilestone);
                ProjectMilestones.Add(newMilestone);
                await _notificationService.ShowToastAsync("Success", $"Milestone '{newMilestone.Name}' has been created.");
            }
            else
            {
                // Updating existing milestone
                var updatedMilestone = await _projectService.UpdateMilestoneAsync(CurrentMilestone);
                var index = ProjectMilestones.ToList().FindIndex(m => m.Id == updatedMilestone.Id);
                if (index >= 0)
                {
                    ProjectMilestones[index] = updatedMilestone;
                }
                await _notificationService.ShowToastAsync("Success", $"Milestone '{updatedMilestone.Name}' has been updated.");
            }

            IsMilestoneEditMode = false;
        }, "Saving milestone...");
    }

    private async Task DeleteMilestoneAsync()
    {
        if (SelectedMilestone == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _projectService.DeleteMilestoneAsync(SelectedMilestone.Id);
            if (result)
            {
                ProjectMilestones.Remove(SelectedMilestone);
                await _notificationService.ShowToastAsync("Success", $"Milestone '{SelectedMilestone.Name}' has been deleted.");
                SelectedMilestone = null;
            }
        }, "Deleting milestone...");
    }

    private async Task CompleteMilestoneAsync()
    {
        if (SelectedMilestone == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _projectService.CompleteMilestoneAsync(SelectedMilestone.Id);
            if (result)
            {
                SelectedMilestone.IsCompleted = true;
                SelectedMilestone.CompletedDate = DateTime.UtcNow;
                await _notificationService.ShowToastAsync("Success", $"Milestone '{SelectedMilestone.Name}' has been completed.");
            }
        }, "Completing milestone...");
    }

    #endregion

    #region Additional Operations

    private async Task RefreshDataAsync()
    {
        await LoadDepartmentsAsync();
        await LoadEmployeesAsync();
        await LoadProjectsAsync();
        await _notificationService.ShowToastAsync("Refreshed", "Project data has been updated.");
    }

    #endregion

    #region Validation

    protected override void ValidateProperty(string propertyName)
    {
        base.ValidateProperty(propertyName);

        switch (propertyName)
        {
            case nameof(CurrentProject.Name):
                if (string.IsNullOrWhiteSpace(CurrentProject.Name))
                    AddError(propertyName, "Project name is required.");
                break;

            case nameof(CurrentProject.Code):
                if (string.IsNullOrWhiteSpace(CurrentProject.Code))
                    AddError(propertyName, "Project code is required.");
                break;

            case nameof(CurrentProject.Budget):
                if (CurrentProject.Budget < 0)
                    AddError(propertyName, "Budget cannot be negative.");
                break;

            case nameof(CurrentProject.ActualCost):
                if (CurrentProject.ActualCost < 0)
                    AddError(propertyName, "Actual cost cannot be negative.");
                break;
        }
    }

    #endregion
} 