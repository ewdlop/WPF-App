using Microsoft.Extensions.Logging;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    public DashboardViewModel(ILogger<DashboardViewModel> logger) : base(logger)
    {
        _logger.LogInformation("DashboardViewModel initialized");
    }

    // TODO: Implement dashboard functionality
    // - KPI widgets
    // - Charts and graphs
    // - Recent activities
    // - Quick actions
} 