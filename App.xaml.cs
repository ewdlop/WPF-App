using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
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

        // Register Windows
        services.AddTransient<MainWindow>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EmployeeViewModel>();
        services.AddTransient<ProjectViewModel>();
        services.AddTransient<DepartmentViewModel>();

        // Register Core Services
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        // Register Database Context (will be implemented later)
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Register AutoMapper (will be configured later)
        // services.AddAutoMapper(typeof(App));

        // Register other services
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddLogging();

        // Register HTTP Client for external API calls (if needed)
        services.AddHttpClient();
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
