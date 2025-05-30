using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly IConfiguration _configuration;
    private string _currentUser = "Admin User";
    private string _applicationTitle = "ModernWPF Enterprise Manager";
    private string _currentSection = "Welcome";

    public MainViewModel(ILogger<MainViewModel> logger, IConfiguration configuration) 
        : base(logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
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
        // Load recent activities
        await LoadRecentActivitiesAsync();
        
        // Load quick statistics
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
            
            // Simulate loading section-specific data
            await Task.Delay(500); // Simulate async operation
            
            // In a real application, you would load section-specific data here
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
                // Add other sections as needed
            }
        }, $"Loading {sectionName}...");
    }

    private async Task LoadRecentActivitiesAsync()
    {
        // Simulate loading recent activities
        await Task.Delay(200);
        
        RecentActivities.Clear();
        RecentActivities.Add("User login at " + DateTime.Now.AddMinutes(-5).ToString("HH:mm"));
        RecentActivities.Add("Project 'Website Redesign' updated");
        RecentActivities.Add("New employee 'John Doe' added");
        RecentActivities.Add("Monthly report generated");
        RecentActivities.Add("Backup completed successfully");
        
        _logger.LogInformation("Recent activities loaded: {Count} items", RecentActivities.Count);
    }

    private async Task LoadQuickStatsAsync()
    {
        // Simulate loading quick statistics
        await Task.Delay(300);
        
        QuickStats.Clear();
        QuickStats.Add(new QuickStatItem { Title = "Total Employees", Value = "156", Icon = "👥", Trend = "+12" });
        QuickStats.Add(new QuickStatItem { Title = "Active Projects", Value = "23", Icon = "💼", Trend = "+3" });
        QuickStats.Add(new QuickStatItem { Title = "Departments", Value = "8", Icon = "🏢", Trend = "0" });
        QuickStats.Add(new QuickStatItem { Title = "This Month Tasks", Value = "89", Icon = "✅", Trend = "+15" });
        
        _logger.LogInformation("Quick stats loaded: {Count} items", QuickStats.Count);
    }

    private async Task LoadDashboardDataAsync()
    {
        // Load dashboard-specific data
        await Task.Delay(100);
        _logger.LogInformation("Dashboard data loaded");
    }

    private async Task LoadEmployeeDataAsync()
    {
        // Load employee-specific data
        await Task.Delay(100);
        _logger.LogInformation("Employee data loaded");
    }

    private async Task LoadProjectDataAsync()
    {
        // Load project-specific data
        await Task.Delay(100);
        _logger.LogInformation("Project data loaded");
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