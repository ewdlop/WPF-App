using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        try
        {
            var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
            
            // Create a simple view to display dashboard data
            var dashboardView = CreateDashboardView(dashboardViewModel);
            MainContentFrame.Content = dashboardView;
            
            // Initialize the ViewModel
            _ = dashboardViewModel.InitializeAsync();
            
            _logger.LogInformation("Dashboard view loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard view");
            ShowPlaceholderContent("Dashboard", "📊", "Error loading dashboard. Please try again.");
        }
    }

    private void LoadEmployeesView()
    {
        try
        {
            var employeeViewModel = _serviceProvider.GetRequiredService<EmployeeViewModel>();
            
            // Create a simple view to display employee data
            var employeeView = CreateEmployeeView(employeeViewModel);
            MainContentFrame.Content = employeeView;
            
            // Initialize the ViewModel
            _ = employeeViewModel.InitializeAsync();
            
            _logger.LogInformation("Employee view loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load employee view");
            ShowPlaceholderContent("Employee Management", "👥", "Error loading employees. Please try again.");
        }
    }

    private void LoadProjectsView()
    {
        try
        {
            var projectViewModel = _serviceProvider.GetRequiredService<ProjectViewModel>();
            
            // Create a simple view to display project data
            var projectView = CreateProjectView(projectViewModel);
            MainContentFrame.Content = projectView;
            
            // Initialize the ViewModel
            _ = projectViewModel.InitializeAsync();
            
            _logger.LogInformation("Project view loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project view");
            ShowPlaceholderContent("Project Management", "💼", "Error loading projects. Please try again.");
        }
    }

    private void LoadDepartmentsView()
    {
        try
        {
            var departmentViewModel = _serviceProvider.GetRequiredService<DepartmentViewModel>();
            
            // Create a simple view to display department data
            var departmentView = CreateDepartmentView(departmentViewModel);
            MainContentFrame.Content = departmentView;
            
            // Initialize the ViewModel
            _ = departmentViewModel.InitializeAsync();
            
            _logger.LogInformation("Department view loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load department view");
            ShowPlaceholderContent("Department Management", "🏢", "Error loading departments. Please try again.");
        }
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

    #region View Creation Methods

    private FrameworkElement CreateDashboardView(DashboardViewModel viewModel)
    {
        var grid = new Grid();
        grid.DataContext = viewModel;

        // Create dashboard content
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(24)
        };

        // Title
        var titleBlock = new TextBlock
        {
            Text = "Dashboard",
            FontSize = 32,
            FontWeight = FontWeights.Light,
            Margin = new Thickness(0, 0, 0, 24)
        };
        stackPanel.Children.Add(titleBlock);

        // KPI Cards
        var kpiPanel = new UniformGrid
        {
            Rows = 2,
            Columns = 4,
            Margin = new Thickness(0, 0, 0, 24)
        };

        // Create KPI cards
        kpiPanel.Children.Add(CreateKpiCard("Total Employees", "{Binding TotalEmployees}", "👥"));
        kpiPanel.Children.Add(CreateKpiCard("Active Projects", "{Binding ActiveProjects}", "💼"));
        kpiPanel.Children.Add(CreateKpiCard("Total Departments", "{Binding TotalDepartments}", "🏢"));
        kpiPanel.Children.Add(CreateKpiCard("Budget Utilization", "{Binding BudgetUtilization:F1}%", "💰"));
        kpiPanel.Children.Add(CreateKpiCard("Completed Projects", "{Binding CompletedProjects}", "✅"));
        kpiPanel.Children.Add(CreateKpiCard("Overdue Projects", "{Binding OverdueProjects}", "⚠️"));
        kpiPanel.Children.Add(CreateKpiCard("Upcoming Milestones", "{Binding UpcomingMilestones}", "📅"));
        kpiPanel.Children.Add(CreateKpiCard("Total Budget", "{Binding TotalBudget:C}", "💵"));

        stackPanel.Children.Add(kpiPanel);

        // Recent Projects Section
        var recentProjectsTitle = new TextBlock
        {
            Text = "Recent Projects",
            FontSize = 20,
            FontWeight = FontWeights.Medium,
            Margin = new Thickness(0, 24, 0, 12)
        };
        stackPanel.Children.Add(recentProjectsTitle);

        var projectsDataGrid = new DataGrid
        {
            ItemsSource = viewModel.RecentProjects,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 300,
            Margin = new Thickness(0, 0, 0, 24)
        };

        projectsDataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
        projectsDataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status") });
        projectsDataGrid.Columns.Add(new DataGridTextColumn { Header = "Progress", Binding = new System.Windows.Data.Binding("ProgressPercentage") });
        projectsDataGrid.Columns.Add(new DataGridTextColumn { Header = "Budget", Binding = new System.Windows.Data.Binding("Budget") });

        stackPanel.Children.Add(projectsDataGrid);

        scrollViewer.Content = stackPanel;
        grid.Children.Add(scrollViewer);

        return grid;
    }

    private FrameworkElement CreateEmployeeView(EmployeeViewModel viewModel)
    {
        var grid = new Grid();
        grid.DataContext = viewModel;

        // Create main layout
        var dockPanel = new DockPanel
        {
            Margin = new Thickness(24)
        };

        // Top toolbar
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Employee Management",
            FontSize = 28,
            FontWeight = FontWeights.Light,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 24, 0)
        };
        toolbar.Children.Add(titleBlock);

        var searchBox = new TextBox
        {
            Text = viewModel.SearchText,
            Width = 300,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        searchBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("SearchText"));
        toolbar.Children.Add(searchBox);

        var searchButton = new Button
        {
            Content = "Search",
            Command = viewModel.SearchCommand,
            Margin = new Thickness(0, 0, 12, 0)
        };
        toolbar.Children.Add(searchButton);

        var addButton = new Button
        {
            Content = "Add Employee",
            Command = viewModel.AddEmployeeCommand
        };
        toolbar.Children.Add(addButton);

        DockPanel.SetDock(toolbar, Dock.Top);
        dockPanel.Children.Add(toolbar);

        // Statistics panel
        var statsPanel = new UniformGrid
        {
            Columns = 4,
            Margin = new Thickness(0, 0, 0, 16)
        };

        statsPanel.Children.Add(CreateKpiCard("Total", "{Binding TotalEmployees}", "👥"));
        statsPanel.Children.Add(CreateKpiCard("Active", "{Binding ActiveEmployees}", "✅"));
        statsPanel.Children.Add(CreateKpiCard("Avg Salary", "{Binding AverageSalary:C}", "💰"));
        statsPanel.Children.Add(CreateKpiCard("New Hires", "{Binding NewHiresThisMonth}", "📅"));

        DockPanel.SetDock(statsPanel, Dock.Top);
        dockPanel.Children.Add(statsPanel);

        // Data grid
        var dataGrid = new DataGrid
        {
            ItemsSource = viewModel.EmployeesView,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectedItem = viewModel.SelectedEmployee
        };

        dataGrid.SetBinding(DataGrid.SelectedItemProperty, new System.Windows.Data.Binding("SelectedEmployee"));

        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Employee #", Binding = new System.Windows.Data.Binding("EmployeeNumber") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("FullName") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Position", Binding = new System.Windows.Data.Binding("Position") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Salary", Binding = new System.Windows.Data.Binding("Salary") });
        dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Active", Binding = new System.Windows.Data.Binding("IsActive") });

        dockPanel.Children.Add(dataGrid);
        grid.Children.Add(dockPanel);

        return grid;
    }

    private FrameworkElement CreateProjectView(ProjectViewModel viewModel)
    {
        var grid = new Grid();
        grid.DataContext = viewModel;

        var dockPanel = new DockPanel
        {
            Margin = new Thickness(24)
        };

        // Top toolbar
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Project Management",
            FontSize = 28,
            FontWeight = FontWeights.Light,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 24, 0)
        };
        toolbar.Children.Add(titleBlock);

        var searchBox = new TextBox
        {
            Text = viewModel.SearchText,
            Width = 300,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        searchBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("SearchText"));
        toolbar.Children.Add(searchBox);

        var addButton = new Button
        {
            Content = "Add Project",
            Command = viewModel.AddProjectCommand
        };
        toolbar.Children.Add(addButton);

        DockPanel.SetDock(toolbar, Dock.Top);
        dockPanel.Children.Add(toolbar);

        // Statistics panel
        var statsPanel = new UniformGrid
        {
            Columns = 4,
            Margin = new Thickness(0, 0, 0, 16)
        };

        statsPanel.Children.Add(CreateKpiCard("Total", "{Binding TotalProjects}", "💼"));
        statsPanel.Children.Add(CreateKpiCard("Active", "{Binding ActiveProjects}", "🔄"));
        statsPanel.Children.Add(CreateKpiCard("Completed", "{Binding CompletedProjects}", "✅"));
        statsPanel.Children.Add(CreateKpiCard("Overdue", "{Binding OverdueProjects}", "⚠️"));

        DockPanel.SetDock(statsPanel, Dock.Top);
        dockPanel.Children.Add(statsPanel);

        // Data grid
        var dataGrid = new DataGrid
        {
            ItemsSource = viewModel.ProjectsView,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectedItem = viewModel.SelectedProject
        };

        dataGrid.SetBinding(DataGrid.SelectedItemProperty, new System.Windows.Data.Binding("SelectedProject"));

        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Code", Binding = new System.Windows.Data.Binding("Code") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Priority", Binding = new System.Windows.Data.Binding("Priority") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Progress", Binding = new System.Windows.Data.Binding("ProgressPercentage") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Budget", Binding = new System.Windows.Data.Binding("Budget") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "End Date", Binding = new System.Windows.Data.Binding("EndDate") });

        dockPanel.Children.Add(dataGrid);
        grid.Children.Add(dockPanel);

        return grid;
    }

    private FrameworkElement CreateDepartmentView(DepartmentViewModel viewModel)
    {
        var grid = new Grid();
        grid.DataContext = viewModel;

        var dockPanel = new DockPanel
        {
            Margin = new Thickness(24)
        };

        // Top toolbar
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Department Management",
            FontSize = 28,
            FontWeight = FontWeights.Light,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 24, 0)
        };
        toolbar.Children.Add(titleBlock);

        var searchBox = new TextBox
        {
            Text = viewModel.SearchText,
            Width = 300,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        searchBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("SearchText"));
        toolbar.Children.Add(searchBox);

        var addButton = new Button
        {
            Content = "Add Department",
            Command = viewModel.AddDepartmentCommand
        };
        toolbar.Children.Add(addButton);

        DockPanel.SetDock(toolbar, Dock.Top);
        dockPanel.Children.Add(toolbar);

        // Data grid
        var dataGrid = new DataGrid
        {
            ItemsSource = viewModel.DepartmentsView,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectedItem = viewModel.SelectedDepartment
        };

        dataGrid.SetBinding(DataGrid.SelectedItemProperty, new System.Windows.Data.Binding("SelectedDepartment"));

        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Code", Binding = new System.Windows.Data.Binding("Code") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Description", Binding = new System.Windows.Data.Binding("Description") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Budget", Binding = new System.Windows.Data.Binding("Budget") });
        dataGrid.Columns.Add(new DataGridCheckBoxColumn { Header = "Active", Binding = new System.Windows.Data.Binding("IsActive") });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "Created", Binding = new System.Windows.Data.Binding("CreatedAt") });

        dockPanel.Children.Add(dataGrid);
        grid.Children.Add(dockPanel);

        return grid;
    }

    private MaterialDesignThemes.Wpf.Card CreateKpiCard(string title, string bindingPath, string icon)
    {
        var card = new MaterialDesignThemes.Wpf.Card
        {
            Padding = new Thickness(16),
            Margin = new Thickness(8)
        };

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var valueText = new TextBlock
        {
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4)
        };

        var binding = new System.Windows.Data.Binding(bindingPath.Replace("{Binding ", "").Replace("}", ""));
        if (bindingPath.Contains(":C"))
            binding.StringFormat = "C";
        else if (bindingPath.Contains(":F1"))
            binding.StringFormat = "F1";
        
        valueText.SetBinding(TextBlock.TextProperty, binding);

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.7
        };

        stackPanel.Children.Add(iconText);
        stackPanel.Children.Add(valueText);
        stackPanel.Children.Add(titleText);

        card.Content = stackPanel;
        return card;
    }

    #endregion

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