using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IProjectService _projectService;
    private readonly IDepartmentService _departmentService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // KPI Properties
    private int _totalEmployees;
    private int _activeEmployees;
    private int _totalProjects;
    private int _activeProjects;
    private int _completedProjects;
    private int _totalDepartments;
    private decimal _totalBudget;
    private decimal _budgetUtilization;
    private int _overdueProjects;
    private int _upcomingMilestones;

    public DashboardViewModel(ILogger<DashboardViewModel> logger, 
        IEmployeeService employeeService, IProjectService projectService,
        IDepartmentService departmentService, IAuditService auditService,
        INotificationService notificationService) 
        : base(logger)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Initialize collections
        RecentProjects = new ObservableCollection<Project>();
        TopPerformingEmployees = new ObservableCollection<Employee>();
        ProjectStatusDistribution = new ObservableCollection<StatusDistributionItem>();
        DepartmentBudgetOverview = new ObservableCollection<DepartmentBudgetItem>();
        RecentActivities = new ObservableCollection<AuditLog>();
        CriticalProjects = new ObservableCollection<Project>();
        OverdueMilestones = new ObservableCollection<ProjectMilestone>();

        // Initialize commands
        RefreshDashboardCommand = new AsyncRelayCommand(RefreshDashboardAsync, () => !IsBusy);
        ViewProjectDetailsCommand = new RelayCommand<int?>(ViewProjectDetails);
        ViewEmployeeDetailsCommand = new RelayCommand<int?>(ViewEmployeeDetails);
        CreateQuickProjectCommand = new AsyncRelayCommand(CreateQuickProjectAsync, () => !IsBusy);
        AddQuickEmployeeCommand = new AsyncRelayCommand(AddQuickEmployeeAsync, () => !IsBusy);

        _logger.LogInformation("DashboardViewModel initialized");
    }

    #region KPI Properties

    public int TotalEmployees
    {
        get => _totalEmployees;
        set => SetProperty(ref _totalEmployees, value);
    }

    public int ActiveEmployees
    {
        get => _activeEmployees;
        set => SetProperty(ref _activeEmployees, value);
    }

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

    public int TotalDepartments
    {
        get => _totalDepartments;
        set => SetProperty(ref _totalDepartments, value);
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

    public int OverdueProjects
    {
        get => _overdueProjects;
        set => SetProperty(ref _overdueProjects, value);
    }

    public int UpcomingMilestones
    {
        get => _upcomingMilestones;
        set => SetProperty(ref _upcomingMilestones, value);
    }

    #endregion

    #region Collections

    public ObservableCollection<Project> RecentProjects { get; }
    public ObservableCollection<Employee> TopPerformingEmployees { get; }
    public ObservableCollection<StatusDistributionItem> ProjectStatusDistribution { get; }
    public ObservableCollection<DepartmentBudgetItem> DepartmentBudgetOverview { get; }
    public ObservableCollection<AuditLog> RecentActivities { get; }
    public ObservableCollection<Project> CriticalProjects { get; }
    public ObservableCollection<ProjectMilestone> OverdueMilestones { get; }

    #endregion

    #region Commands

    public ICommand RefreshDashboardCommand { get; }
    public ICommand ViewProjectDetailsCommand { get; }
    public ICommand ViewEmployeeDetailsCommand { get; }
    public ICommand CreateQuickProjectCommand { get; }
    public ICommand AddQuickEmployeeCommand { get; }

    #endregion

    #region Initialization

    public override async Task InitializeAsync()
    {
        await LoadDashboardDataAsync();
    }

    public override async Task LoadDataAsync()
    {
        await LoadDashboardDataAsync();
    }

    #endregion

    #region Data Loading

    private async Task LoadDashboardDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            // Load KPIs
            await LoadKPIsAsync();

            // Load dashboard collections
            await LoadRecentProjectsAsync();
            await LoadTopPerformingEmployeesAsync();
            await LoadProjectStatusDistributionAsync();
            await LoadDepartmentBudgetOverviewAsync();
            await LoadRecentActivitiesAsync();
            await LoadCriticalProjectsAsync();
            await LoadOverdueMilestonesAsync();

            _logger.LogInformation("Dashboard data loaded successfully");
        }, "Loading dashboard data...");
    }

    private async Task LoadKPIsAsync()
    {
        try
        {
            // Employee statistics
            TotalEmployees = await _employeeService.GetTotalEmployeeCountAsync();
            ActiveEmployees = await _employeeService.GetActiveEmployeeCountAsync();

            // Project statistics
            TotalProjects = await _projectService.GetTotalProjectCountAsync();
            ActiveProjects = await _projectService.GetActiveProjectCountAsync();
            CompletedProjects = await _projectService.GetCompletedProjectCountAsync();

            // Department statistics
            var departments = await _departmentService.GetAllDepartmentsAsync();
            TotalDepartments = departments.Count(d => d.IsActive);

            // Budget statistics
            TotalBudget = await _projectService.GetTotalBudgetAllocationAsync();
            BudgetUtilization = await _projectService.GetBudgetUtilizationAsync();

            // Critical metrics
            var overdueProjects = await _projectService.GetOverdueProjectsAsync();
            OverdueProjects = overdueProjects.Count();

            var upcomingMilestones = await _projectService.GetUpcomingMilestonesAsync(7);
            UpcomingMilestones = upcomingMilestones.Count();

            _logger.LogInformation("KPIs loaded: {TotalEmployees} employees, {TotalProjects} projects", 
                TotalEmployees, TotalProjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load KPIs");
            SetErrorMessage("Failed to load dashboard statistics");
        }
    }

    private async Task LoadRecentProjectsAsync()
    {
        try
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var recentProjects = projects.OrderByDescending(p => p.CreatedAt).Take(5);

            RecentProjects.Clear();
            foreach (var project in recentProjects)
            {
                RecentProjects.Add(project);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent projects");
        }
    }

    private async Task LoadTopPerformingEmployeesAsync()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var topEmployees = employees.Where(e => e.IsActive)
                                      .OrderByDescending(e => e.HireDate) // Could be performance metric
                                      .Take(5);

            TopPerformingEmployees.Clear();
            foreach (var employee in topEmployees)
            {
                TopPerformingEmployees.Add(employee);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load top performing employees");
        }
    }

    private async Task LoadProjectStatusDistributionAsync()
    {
        try
        {
            var distribution = await _projectService.GetProjectStatusDistributionAsync();

            ProjectStatusDistribution.Clear();
            foreach (var item in distribution)
            {
                ProjectStatusDistribution.Add(new StatusDistributionItem
                {
                    Status = item.Key.ToString(),
                    Count = item.Value,
                    Percentage = TotalProjects > 0 ? (double)item.Value / TotalProjects * 100 : 0
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project status distribution");
        }
    }

    private async Task LoadDepartmentBudgetOverviewAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();

            DepartmentBudgetOverview.Clear();
            foreach (var dept in departments.Where(d => d.IsActive))
            {
                var utilization = await _departmentService.GetDepartmentBudgetUtilizationAsync(dept.Id);
                DepartmentBudgetOverview.Add(new DepartmentBudgetItem
                {
                    DepartmentName = dept.Name,
                    Budget = dept.Budget,
                    Utilization = utilization,
                    IsOverBudget = utilization > 100
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load department budget overview");
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            var activities = await _auditService.GetRecentAuditLogsAsync(10);

            RecentActivities.Clear();
            foreach (var activity in activities)
            {
                RecentActivities.Add(activity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent activities");
        }
    }

    private async Task LoadCriticalProjectsAsync()
    {
        try
        {
            var criticalProjects = await _projectService.GetMostCriticalProjectsAsync(5);

            CriticalProjects.Clear();
            foreach (var project in criticalProjects)
            {
                CriticalProjects.Add(project);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load critical projects");
        }
    }

    private async Task LoadOverdueMilestonesAsync()
    {
        try
        {
            var overdueMilestones = await _projectService.GetOverdueMilestonesAsync();

            OverdueMilestones.Clear();
            foreach (var milestone in overdueMilestones.Take(5))
            {
                OverdueMilestones.Add(milestone);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load overdue milestones");
        }
    }

    #endregion

    #region Command Implementations

    private async Task RefreshDashboardAsync()
    {
        await LoadDashboardDataAsync();
        await _notificationService.ShowToastAsync("Dashboard Refreshed", "All data has been updated successfully.");
    }

    private void ViewProjectDetails(int? projectId)
    {
        if (projectId.HasValue)
        {
            _logger.LogInformation("Viewing project details for ID: {ProjectId}", projectId.Value);
            // TODO: Navigate to project details view
            // This would typically be handled by a navigation service
        }
    }

    private void ViewEmployeeDetails(int? employeeId)
    {
        if (employeeId.HasValue)
        {
            _logger.LogInformation("Viewing employee details for ID: {EmployeeId}", employeeId.Value);
            // TODO: Navigate to employee details view
            // This would typically be handled by a navigation service
        }
    }

    private async Task CreateQuickProjectAsync()
    {
        await ExecuteAsync(async () =>
        {
            // This would typically open a quick project creation dialog
            _logger.LogInformation("Quick project creation initiated");
            await _notificationService.ShowToastAsync("Quick Action", "Project creation dialog would open here.");
        }, "Creating project...");
    }

    private async Task AddQuickEmployeeAsync()
    {
        await ExecuteAsync(async () =>
        {
            // This would typically open a quick employee addition dialog
            _logger.LogInformation("Quick employee addition initiated");
            await _notificationService.ShowToastAsync("Quick Action", "Employee addition dialog would open here.");
        }, "Adding employee...");
    }

    #endregion
}

// Helper classes for dashboard data binding
public class StatusDistributionItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class DepartmentBudgetItem
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Utilization { get; set; }
    public bool IsOverBudget { get; set; }
} 