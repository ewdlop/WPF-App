using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp2.ViewModels;

namespace WpfApp2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private readonly DispatcherTimer _timeTimer;
    private bool _isNavigationDrawerCollapsed = false;

    public MainWindow(IServiceProvider serviceProvider, ILogger<MainWindow> logger)
    {
        InitializeComponent();
        
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize timer for status bar clock
        _timeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timeTimer.Tick += TimeTimer_Tick;
        _timeTimer.Start();

        // Update initial time display
        UpdateTimeDisplay();

        // Set initial status
        UpdateStatus("Application loaded successfully");

        _logger.LogInformation("MainWindow initialized");

        // Load the main view model
        LoadMainViewModel();
    }

    private void LoadMainViewModel()
    {
        try
        {
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            DataContext = mainViewModel;
            
            // Initialize the view model asynchronously
            Dispatcher.BeginInvoke(async () =>
            {
                await mainViewModel.InitializeAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load MainViewModel");
            UpdateStatus("Failed to load application data");
        }
    }

    private void TimeTimer_Tick(object? sender, EventArgs e)
    {
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        TimeTextBlock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void UpdateStatus(string message)
    {
        StatusTextBlock.Text = message;
        _logger.LogInformation("Status updated: {Message}", message);
    }

    private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleNavigationDrawer();
    }

    private void ToggleNavigationDrawer()
    {
        if (_isNavigationDrawerCollapsed)
        {
            // Expand drawer
            NavigationDrawer.Width = 280;
            _isNavigationDrawerCollapsed = false;
            UpdateStatus("Navigation expanded");
        }
        else
        {
            // Collapse drawer
            NavigationDrawer.Width = 60;
            _isNavigationDrawerCollapsed = true;
            UpdateStatus("Navigation collapsed");
        }
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            NavigateToSection(tag);
        }
    }

    private void NavigateToSection(string sectionName)
    {
        try
        {
            _logger.LogInformation("Navigating to section: {SectionName}", sectionName);
            
            // Hide welcome card when navigating to a specific section
            WelcomeCard.Visibility = Visibility.Collapsed;
            
            // Update visual state of navigation buttons
            UpdateNavigationButtonStates(sectionName);
            
            switch (sectionName.ToLower())
            {
                case "dashboard":
                    LoadDashboardView();
                    break;
                    
                case "employees":
                    LoadEmployeesView();
                    break;
                    
                case "projects":
                    LoadProjectsView();
                    break;
                    
                case "departments":
                    LoadDepartmentsView();
                    break;
                    
                case "reports":
                    LoadReportsView();
                    break;
                    
                case "auditlogs":
                    LoadAuditLogsView();
                    break;
                    
                case "settings":
                    LoadSettingsView();
                    break;
                    
                case "help":
                    LoadHelpView();
                    break;
                    
                default:
                    _logger.LogWarning("Unknown section: {SectionName}", sectionName);
                    UpdateStatus($"Unknown section: {sectionName}");
                    ShowWelcomeCard();
                    break;
            }
            
            UpdateStatus($"Navigated to {sectionName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to section: {SectionName}", sectionName);
            UpdateStatus($"Error loading {sectionName}");
        }
    }

    private void UpdateNavigationButtonStates(string activeSectionName)
    {
        // Reset all button styles to default
        var buttons = new[]
        {
            DashboardButton, EmployeesButton, ProjectsButton, DepartmentsButton,
            ReportsButton, AuditLogsButton, SettingsButton, HelpButton
        };

        foreach (var button in buttons)
        {
            button.Style = (Style)FindResource("NavigationButtonStyle");
        }

        // Highlight the active button (simplified - in a real app you'd have a proper active style)
        var activeButton = activeSectionName.ToLower() switch
        {
            "dashboard" => DashboardButton,
            "employees" => EmployeesButton,
            "projects" => ProjectsButton,
            "departments" => DepartmentsButton,
            "reports" => ReportsButton,
            "auditlogs" => AuditLogsButton,
            "settings" => SettingsButton,
            "help" => HelpButton,
            _ => null
        };

        if (activeButton != null)
        {
            // In a real application, you would set an "Active" style here
            activeButton.Opacity = 0.7;
        }
    }

    private void LoadDashboardView()
    {
        // TODO: Create and load DashboardView
        ShowPlaceholderContent("Dashboard", "📊", "Welcome to the Dashboard! Here you'll see key metrics, charts, and KPIs for your organization.");
    }

    private void LoadEmployeesView()
    {
        // TODO: Create and load EmployeesView
        ShowPlaceholderContent("Employee Management", "👥", "Manage your organization's employees, view profiles, track performance, and handle HR operations.");
    }

    private void LoadProjectsView()
    {
        // TODO: Create and load ProjectsView
        ShowPlaceholderContent("Project Management", "💼", "Track project progress, manage timelines, allocate resources, and monitor deliverables.");
    }

    private void LoadDepartmentsView()
    {
        // TODO: Create and load DepartmentsView
        ShowPlaceholderContent("Department Management", "🏢", "Organize departments, manage budgets, and track departmental performance metrics.");
    }

    private void LoadReportsView()
    {
        // TODO: Create and load ReportsView
        ShowPlaceholderContent("Reports & Analytics", "📈", "Generate comprehensive reports, analyze trends, and export data for decision making.");
    }

    private void LoadAuditLogsView()
    {
        // TODO: Create and load AuditLogsView
        ShowPlaceholderContent("Audit Logs", "📝", "Review system activities, track changes, and maintain compliance with audit trails.");
    }

    private void LoadSettingsView()
    {
        // TODO: Create and load SettingsView
        ShowPlaceholderContent("Application Settings", "⚙️", "Configure application preferences, manage user settings, and customize your experience.");
    }

    private void LoadHelpView()
    {
        // TODO: Create and load HelpView
        ShowPlaceholderContent("Help & Support", "❓", "Access documentation, tutorials, and support resources to make the most of the application.");
    }

    private void ShowPlaceholderContent(string title, string icon, string description)
    {
        // Create a simple placeholder content for navigation
        var placeholderContent = new Grid();
        
        var card = new MaterialDesignThemes.Wpf.Card
        {
            Padding = new Thickness(32),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 600
        };

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 72,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 28,
            FontWeight = FontWeights.Light,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var descriptionText = new TextBlock
        {
            Text = description,
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            LineHeight = 24,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var comingSoonText = new TextBlock
        {
            Text = "This feature is coming soon in future updates.",
            FontSize = 14,
            FontStyle = FontStyles.Italic,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.7
        };

        stackPanel.Children.Add(iconText);
        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(descriptionText);
        stackPanel.Children.Add(comingSoonText);
        
        card.Content = stackPanel;
        placeholderContent.Children.Add(card);

        MainContentFrame.Content = placeholderContent;
    }

    private void ShowWelcomeCard()
    {
        MainContentFrame.Content = null;
        WelcomeCard.Visibility = Visibility.Visible;
    }

    private void GetStartedButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to Dashboard when user clicks "Get Started"
        NavigateToSection("Dashboard");
    }

    protected override void OnClosed(EventArgs e)
    {
        _timeTimer?.Stop();
        _logger.LogInformation("MainWindow closed");
        base.OnClosed(e);
    }
}