using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.ViewModels.Base;
using WpfApp2.Services.Interfaces;

namespace WpfApp2.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IConfiguration _configuration;
    private readonly IEmployeeService _employeeService;
    private readonly IProjectService _projectService;
    private readonly IDepartmentService _departmentService;
    private readonly IAuditService _auditService;
    
    private string _currentUser = "Admin User";
    private string _applicationTitle = "ModernWPF Enterprise Manager";
    private string _currentSection = "Welcome";

    public MainViewModel(ILogger<MainViewModel> logger, IConfiguration configuration,
        IEmployeeService employeeService, IProjectService projectService, 
        IDepartmentService departmentService, IAuditService auditService) 
        : base(logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        
        // Initialize commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        NavigateCommand = new RelayCommand<string>(Navigate, CanNavigate);
        
        // Initialize collections
        RecentActivities = new ObservableCollection<string>();
        QuickStats = new ObservableCollection<QuickStatItem>();
        
        _logger.LogInformation("MainViewModel initialized");
    }

    #region Properties

    public string CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string ApplicationTitle
    {
        get => _applicationTitle;
        set => SetProperty(ref _applicationTitle, value);
    }

    public string CurrentSection
    {
        get => _currentSection;
        set => SetProperty(ref _currentSection, value);
    }

    public ObservableCollection<string> RecentActivities { get; }
    public ObservableCollection<QuickStatItem> QuickStats { get; }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand NavigateCommand { get; }

    #endregion

    #region Command Implementations

    private void Navigate(string? sectionName)
    {
        if (string.IsNullOrEmpty(sectionName)) return;
        
        CurrentSection = sectionName;
        _logger.LogInformation("Navigated to section: {Section}", sectionName);
        
        // Update any section-specific data here
        _ = LoadSectionDataAsync(sectionName);
    }

    private bool CanNavigate(string? sectionName)
    {
        return !string.IsNullOrEmpty(sectionName) && !IsBusy;
    }

    #endregion

    #region Initialization and Data Loading

    public override async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Initializing MainViewModel...");
            
            // Load configuration settings
            LoadApplicationSettings();
            
            // Load initial data
            await LoadInitialDataAsync();
            
            _logger.LogInformation("MainViewModel initialization completed");
        }, "Initializing application...");
    }

    private void LoadApplicationSettings()
    {
        try
        {
            // Load settings from configuration
            var uiSettings = _configuration.GetSection("UI");
            var appTitle = _configuration["Application:Title"];
            
            if (!string.IsNullOrEmpty(appTitle))
            {
                ApplicationTitle = appTitle;
            }
            
            _logger.LogInformation("Application settings loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load application settings");
        }
    }

    private async Task LoadInitialDataAsync()
    {
        // Load recent activities from audit service
        await LoadRecentActivitiesAsync();
        
        // Load quick statistics from services
        await LoadQuickStatsAsync();
    }

    public override async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            await LoadInitialDataAsync();
        }, "Loading data...");
    }

    private async Task LoadSectionDataAsync(string sectionName)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading data for section: {Section}", sectionName);
            
            // Load section-specific data
            switch (sectionName.ToLower())
            {
                case "dashboard":
                    await LoadDashboardDataAsync();
                    break;
                case "employees":
                    await LoadEmployeeDataAsync();
                    break;
                case "projects":
                    await LoadProjectDataAsync();
                    break;
                case "departments":
                    await LoadDepartmentDataAsync();
                    break;
                // Add other sections as needed
            }
        }, $"Loading {sectionName}...");
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            // Get recent audit logs
            var recentLogs = await _auditService.GetRecentAuditLogsAsync(10);
            
            RecentActivities.Clear();
            foreach (var log in recentLogs.Take(10))
            {
                var timeAgo = GetTimeAgo(log.Timestamp);
                var description = !string.IsNullOrEmpty(log.Description) ? log.Description : $"{log.Action}";
                RecentActivities.Add($"{timeAgo} - {description}");
            }

            // If no audit logs, add some default activities
            if (!RecentActivities.Any())
            {
                RecentActivities.Add($"{GetTimeAgo(DateTime.UtcNow.AddMinutes(-5))} - Application started");
                RecentActivities.Add($"{GetTimeAgo(DateTime.UtcNow.AddMinutes(-10))} - System initialized");
            }
            
            _logger.LogInformation("Recent activities loaded: {Count} items", RecentActivities.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recent activities");
            // Fallback to default activities
            RecentActivities.Clear();
            RecentActivities.Add($"{GetTimeAgo(DateTime.UtcNow)} - Application started");
        }
    }

    private async Task LoadQuickStatsAsync()
    {
        try
        {
            // Get statistics from services
            var totalEmployees = await _employeeService.GetTotalEmployeeCountAsync();
            var activeEmployees = await _employeeService.GetActiveEmployeeCountAsync();
            var totalProjects = await _projectService.GetTotalProjectCountAsync();
            var activeProjects = await _projectService.GetActiveProjectCountAsync();
            var allDepartments = await _departmentService.GetAllDepartmentsAsync();
            var activeDepartments = allDepartments.Count(d => d.IsActive);
            
            QuickStats.Clear();
            QuickStats.Add(new QuickStatItem 
            { 
                Title = "Total Employees", 
                Value = totalEmployees.ToString(), 
                Icon = "👥", 
                Trend = $"+{activeEmployees - (totalEmployees - activeEmployees)}" 
            });
            
            QuickStats.Add(new QuickStatItem 
            { 
                Title = "Active Projects", 
                Value = activeProjects.ToString(), 
                Icon = "💼", 
                Trend = $"+{Math.Max(0, activeProjects - (totalProjects - activeProjects))}" 
            });
            
            QuickStats.Add(new QuickStatItem 
            { 
                Title = "Departments", 
                Value = activeDepartments.ToString(), 
                Icon = "🏢", 
                Trend = "0" 
            });
            
            // Calculate completed tasks/milestones
            var completedMilestones = 0; // Will be calculated when milestone data is available
            QuickStats.Add(new QuickStatItem 
            { 
                Title = "Completed Tasks", 
                Value = completedMilestones.ToString(), 
                Icon = "✅", 
                Trend = "+0" 
            });
            
            _logger.LogInformation("Quick stats loaded: {Count} items", QuickStats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load quick statistics");
            // Fallback to default stats
            QuickStats.Clear();
            QuickStats.Add(new QuickStatItem { Title = "Loading...", Value = "0", Icon = "⏳", Trend = "0" });
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        // Refresh dashboard data
        await LoadQuickStatsAsync();
        await LoadRecentActivitiesAsync();
        _logger.LogInformation("Dashboard data loaded");
    }

    private async Task LoadEmployeeDataAsync()
    {
        // This will be handled by EmployeeViewModel
        _logger.LogInformation("Employee data section loaded");
    }

    private async Task LoadProjectDataAsync()
    {
        // This will be handled by ProjectViewModel
        _logger.LogInformation("Project data section loaded");
    }

    private async Task LoadDepartmentDataAsync()
    {
        // This could be handled by a DepartmentViewModel in the future
        _logger.LogInformation("Department data section loaded");
    }

    #endregion

    #region Helper Methods

    public void UpdateCurrentUser(string userName)
    {
        CurrentUser = userName;
        _logger.LogInformation("Current user updated to: {User}", userName);
    }

    public async Task LogActivityAsync(string activity)
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RecentActivities.Insert(0, $"{DateTime.Now:HH:mm} - {activity}");
                
                // Keep only the latest 10 activities
                while (RecentActivities.Count > 10)
                {
                    RecentActivities.RemoveAt(RecentActivities.Count - 1);
                }
            });
        });
        
        _logger.LogInformation("Activity logged: {Activity}", activity);
    }

    private string GetTimeAgo(DateTime timestamp)
    {
        var timeSpan = DateTime.UtcNow - timestamp;
        
        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        
        return timestamp.ToString("MMM dd");
    }

    #endregion

    #region Cleanup

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogInformation("MainViewModel disposed");
        }
        base.Dispose(disposing);
    }

    #endregion
}

// Helper class for quick statistics
public class QuickStatItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public bool IsPositiveTrend => Trend.StartsWith('+');
} 