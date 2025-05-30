using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class DepartmentViewModel : BaseViewModel
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;
    private readonly IProjectService _projectService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // Collections
    private readonly ObservableCollection<Department> _allDepartments = new();
    private ICollectionView? _departmentsView;

    // Properties
    private Department? _selectedDepartment;
    private Department _currentDepartment = new();
    private bool _isEditMode;
    private string _searchText = string.Empty;
    private bool _showActiveOnly = true;

    // Department details
    private int _employeeCount;
    private int _activeProjectCount;
    private decimal _totalSalaries;
    private decimal _averageSalary;
    private decimal _budgetUtilization;

    public DepartmentViewModel(ILogger<DepartmentViewModel> logger,
        IDepartmentService departmentService, IEmployeeService employeeService,
        IProjectService projectService, IAuditService auditService,
        INotificationService notificationService)
        : base(logger)
    {
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Initialize collections
        DepartmentEmployees = new ObservableCollection<Employee>();
        DepartmentProjects = new ObservableCollection<Project>();
        Managers = new ObservableCollection<Employee>();

        // Initialize commands
        LoadDepartmentsCommand = new AsyncRelayCommand(LoadDepartmentsAsync, () => !IsBusy);
        SearchCommand = new AsyncRelayCommand(SearchDepartmentsAsync, () => !IsBusy);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        AddDepartmentCommand = new RelayCommand(AddDepartment, () => !IsBusy);
        EditDepartmentCommand = new RelayCommand(EditDepartment, () => SelectedDepartment != null && !IsBusy);
        SaveDepartmentCommand = new AsyncRelayCommand(SaveDepartmentAsync, () => !IsBusy && !HasErrors);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditMode);
        DeleteDepartmentCommand = new AsyncRelayCommand(DeleteDepartmentAsync, () => SelectedDepartment != null && !IsBusy);
        RefreshCommand = new AsyncRelayCommand(RefreshDataAsync, () => !IsBusy);
        ActivateDepartmentCommand = new AsyncRelayCommand(ActivateDepartmentAsync, () => SelectedDepartment?.IsActive == false && !IsBusy);
        DeactivateDepartmentCommand = new AsyncRelayCommand(DeactivateDepartmentAsync, () => SelectedDepartment?.IsActive == true && !IsBusy);
        LoadDepartmentDetailsCommand = new AsyncRelayCommand(LoadDepartmentDetailsAsync, () => SelectedDepartment != null && !IsBusy);

        _logger.LogInformation("DepartmentViewModel initialized");
    }

    #region Properties

    public ObservableCollection<Employee> DepartmentEmployees { get; }
    public ObservableCollection<Project> DepartmentProjects { get; }
    public ObservableCollection<Employee> Managers { get; }

    public ICollectionView DepartmentsView
    {
        get
        {
            if (_departmentsView == null)
            {
                _departmentsView = CollectionViewSource.GetDefaultView(_allDepartments);
                _departmentsView.Filter = DepartmentFilter;
                _departmentsView.SortDescriptions.Add(new SortDescription(nameof(Department.Name), ListSortDirection.Ascending));
            }
            return _departmentsView;
        }
    }

    public Department? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (SetProperty(ref _selectedDepartment, value))
            {
                _ = LoadDepartmentDetailsAsync();
            }
        }
    }

    public Department CurrentDepartment
    {
        get => _currentDepartment;
        set => SetProperty(ref _currentDepartment, value);
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
                _departmentsView?.Refresh();
            }
        }
    }

    public bool ShowActiveOnly
    {
        get => _showActiveOnly;
        set
        {
            if (SetProperty(ref _showActiveOnly, value))
            {
                _departmentsView?.Refresh();
            }
        }
    }

    #endregion

    #region Department Detail Properties

    public int EmployeeCount
    {
        get => _employeeCount;
        set => SetProperty(ref _employeeCount, value);
    }

    public int ActiveProjectCount
    {
        get => _activeProjectCount;
        set => SetProperty(ref _activeProjectCount, value);
    }

    public decimal TotalSalaries
    {
        get => _totalSalaries;
        set => SetProperty(ref _totalSalaries, value);
    }

    public decimal AverageSalary
    {
        get => _averageSalary;
        set => SetProperty(ref _averageSalary, value);
    }

    public decimal BudgetUtilization
    {
        get => _budgetUtilization;
        set => SetProperty(ref _budgetUtilization, value);
    }

    #endregion

    #region Commands

    public ICommand LoadDepartmentsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand AddDepartmentCommand { get; }
    public ICommand EditDepartmentCommand { get; }
    public ICommand SaveDepartmentCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteDepartmentCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ActivateDepartmentCommand { get; }
    public ICommand DeactivateDepartmentCommand { get; }
    public ICommand LoadDepartmentDetailsCommand { get; }

    #endregion

    #region Initialization

    public override async Task InitializeAsync()
    {
        await LoadManagersAsync();
        await LoadDepartmentsAsync();
    }

    public override async Task LoadDataAsync()
    {
        await LoadDepartmentsAsync();
    }

    #endregion

    #region Data Loading

    private async Task LoadDepartmentsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            
            _allDepartments.Clear();
            foreach (var department in departments)
            {
                _allDepartments.Add(department);
            }
            
            _logger.LogInformation("Loaded {Count} departments", _allDepartments.Count);
        }, "Loading departments...");
    }

    private async Task LoadManagersAsync()
    {
        try
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            var managers = employees.Where(e => e.Position.Contains("Manager") || 
                                               e.Position.Contains("Director") || 
                                               e.Position.Contains("Lead"));

            Managers.Clear();
            Managers.Add(new Employee { Id = 0, FirstName = "No", LastName = "Manager" }); // No manager option
            
            foreach (var manager in managers)
            {
                Managers.Add(manager);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load managers");
        }
    }

    private async Task LoadDepartmentDetailsAsync()
    {
        if (SelectedDepartment == null) return;

        await ExecuteAsync(async () =>
        {
            // Load department statistics
            var stats = await _departmentService.GetDepartmentStatisticsAsync(SelectedDepartment.Id);
            
            EmployeeCount = stats.ContainsKey("EmployeeCount") ? (int)stats["EmployeeCount"] : 0;
            ActiveProjectCount = stats.ContainsKey("ActiveProjectCount") ? (int)stats["ActiveProjectCount"] : 0;
            TotalSalaries = stats.ContainsKey("TotalSalaries") ? (decimal)stats["TotalSalaries"] : 0;
            AverageSalary = stats.ContainsKey("AverageSalary") ? (decimal)stats["AverageSalary"] : 0;
            BudgetUtilization = stats.ContainsKey("BudgetUtilization") ? (decimal)stats["BudgetUtilization"] : 0;

            // Load department employees
            var employees = await _departmentService.GetDepartmentEmployeesAsync(SelectedDepartment.Id);
            DepartmentEmployees.Clear();
            foreach (var employee in employees)
            {
                DepartmentEmployees.Add(employee);
            }

            // Load department projects
            var projects = await _departmentService.GetDepartmentProjectsAsync(SelectedDepartment.Id);
            DepartmentProjects.Clear();
            foreach (var project in projects)
            {
                DepartmentProjects.Add(project);
            }
        }, "Loading department details...");
    }

    #endregion

    #region Search and Filter

    private async Task SearchDepartmentsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadDepartmentsAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            var searchResults = await _departmentService.SearchDepartmentsAsync(SearchText);
            
            _allDepartments.Clear();
            foreach (var department in searchResults)
            {
                _allDepartments.Add(department);
            }
        }, "Searching departments...");
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        ShowActiveOnly = true;
        _ = LoadDepartmentsAsync();
    }

    private bool DepartmentFilter(object item)
    {
        if (item is not Department department)
            return false;

        // Active filter
        if (ShowActiveOnly && !department.IsActive)
            return false;

        // Text search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            return department.Name.ToLowerInvariant().Contains(searchLower) ||
                   department.Code.ToLowerInvariant().Contains(searchLower) ||
                   (department.Description?.ToLowerInvariant().Contains(searchLower) ?? false);
        }

        return true;
    }

    #endregion

    #region CRUD Operations

    private void AddDepartment()
    {
        CurrentDepartment = new Department
        {
            IsActive = true,
            Budget = 0
        };
        IsEditMode = true;
        SelectedDepartment = null;
    }

    private void EditDepartment()
    {
        if (SelectedDepartment != null)
        {
            // Create a copy for editing
            CurrentDepartment = new Department
            {
                Id = SelectedDepartment.Id,
                Name = SelectedDepartment.Name,
                Code = SelectedDepartment.Code,
                Description = SelectedDepartment.Description,
                Budget = SelectedDepartment.Budget,
                ManagerId = SelectedDepartment.ManagerId,
                IsActive = SelectedDepartment.IsActive
            };
            IsEditMode = true;
        }
    }

    private async Task SaveDepartmentAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                if (CurrentDepartment.Id == 0)
                {
                    // Adding new department
                    var newDepartment = await _departmentService.CreateDepartmentAsync(CurrentDepartment);
                    _allDepartments.Add(newDepartment);
                    await _notificationService.ShowToastAsync("Success", $"Department '{newDepartment.Name}' has been created.");
                }
                else
                {
                    // Updating existing department
                    var updatedDepartment = await _departmentService.UpdateDepartmentAsync(CurrentDepartment);
                    var index = _allDepartments.ToList().FindIndex(d => d.Id == updatedDepartment.Id);
                    if (index >= 0)
                    {
                        _allDepartments[index] = updatedDepartment;
                    }
                    await _notificationService.ShowToastAsync("Success", $"Department '{updatedDepartment.Name}' has been updated.");
                }

                IsEditMode = false;
            }
            catch (ArgumentException ex)
            {
                await _notificationService.ShowToastAsync("Validation Error", ex.Message);
            }
        }, "Saving department...");
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentDepartment = new Department();
        ClearErrors();
    }

    private async Task DeleteDepartmentAsync()
    {
        if (SelectedDepartment == null) return;

        await ExecuteAsync(async () =>
        {
            var canDelete = await _departmentService.CanDeleteDepartmentAsync(SelectedDepartment.Id);
            if (!canDelete)
            {
                await _notificationService.ShowToastAsync("Error", "Cannot delete department. It may have employees or active projects.");
                return;
            }

            var result = await _departmentService.DeleteDepartmentAsync(SelectedDepartment.Id);
            if (result)
            {
                _allDepartments.Remove(SelectedDepartment);
                await _notificationService.ShowToastAsync("Success", $"Department '{SelectedDepartment.Name}' has been deleted.");
                SelectedDepartment = null;
            }
        }, "Deleting department...");
    }

    #endregion

    #region Additional Operations

    private async Task ActivateDepartmentAsync()
    {
        if (SelectedDepartment == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _departmentService.ActivateDepartmentAsync(SelectedDepartment.Id);
            if (result)
            {
                SelectedDepartment.IsActive = true;
                await _notificationService.ShowToastAsync("Success", $"Department '{SelectedDepartment.Name}' has been activated.");
                _departmentsView?.Refresh();
            }
        }, "Activating department...");
    }

    private async Task DeactivateDepartmentAsync()
    {
        if (SelectedDepartment == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _departmentService.DeactivateDepartmentAsync(SelectedDepartment.Id);
            if (result)
            {
                SelectedDepartment.IsActive = false;
                await _notificationService.ShowToastAsync("Success", $"Department '{SelectedDepartment.Name}' has been deactivated.");
                _departmentsView?.Refresh();
            }
        }, "Deactivating department...");
    }

    private async Task RefreshDataAsync()
    {
        await LoadManagersAsync();
        await LoadDepartmentsAsync();
        await _notificationService.ShowToastAsync("Refreshed", "Department data has been updated.");
    }

    #endregion

    #region Validation

    protected override void ValidateProperty(string propertyName)
    {
        base.ValidateProperty(propertyName);

        switch (propertyName)
        {
            case nameof(CurrentDepartment.Name):
                if (string.IsNullOrWhiteSpace(CurrentDepartment.Name))
                    AddError(propertyName, "Department name is required.");
                break;

            case nameof(CurrentDepartment.Code):
                if (string.IsNullOrWhiteSpace(CurrentDepartment.Code))
                    AddError(propertyName, "Department code is required.");
                break;

            case nameof(CurrentDepartment.Budget):
                if (CurrentDepartment.Budget < 0)
                    AddError(propertyName, "Budget cannot be negative.");
                break;
        }
    }

    #endregion
} 