using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using WpfApp2.Data;
using WpfApp2.Data.Repositories;
using WpfApp2.ViewModels;
using WpfApp2.ViewModels.Base;
using WpfApp2.Services;
using WpfApp2.Services.Interfaces;

namespace WpfApp2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Configure and start the host
            _host = CreateHostBuilder(e.Args).Build();
            
            // Initialize database
            await InitializeDatabaseAsync();
            
            await _host.StartAsync();

            // Configure Serilog
            ConfigureSerilog();

            // Show the main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application startup failed: {ex.Message}", "Startup Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
            });
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        services.AddSingleton(configuration);

        // Register Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                              "Data Source=enterprise_manager.db;Cache=Shared";
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            if (configuration.GetValue<bool>("Database:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
            if (configuration.GetValue<bool>("Database:EnableDetailedErrors"))
            {
                options.EnableDetailedErrors();
            }
        });

        // Register Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Windows
        services.AddTransient<MainWindow>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EmployeeViewModel>();
        services.AddTransient<ProjectViewModel>();
        services.AddTransient<DepartmentViewModel>();

        // Register Core Services (updated to use repositories)
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<INotificationService, NotificationService>();
        
        // Note: These services can now be updated to use the repository pattern instead of in-memory storage
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        // Register AutoMapper (for mapping between entities and DTOs if needed)
        // services.AddAutoMapper(typeof(App));

        // Register other services
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddLogging();

        // Register HTTP Client for external API calls (if needed)
        services.AddHttpClient();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _host!.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

            logger.LogInformation("Initializing database...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Database initialization completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize database");
            throw;
        }
    }

    private void ConfigureSerilog()
    {
        var configuration = _host!.Services.GetRequiredService<IConfiguration>();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ModernWPF Enterprise Manager")
            .Enrich.WithProperty("Version", "1.0.0")
            .CreateLogger();

        Log.Information("Application starting up...");
    }
}
