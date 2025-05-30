using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using WpfApp2.Models;
using WpfApp2.Services.Interfaces;
using WpfApp2.ViewModels.Base;

namespace WpfApp2.ViewModels;

public class EmployeeViewModel : BaseViewModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    // Collections
    private readonly ObservableCollection<Employee> _allEmployees = new();
    private ICollectionView? _employeesView;

    // Properties
    private Employee? _selectedEmployee;
    private Employee _currentEmployee = new();
    private bool _isEditMode;
    private string _searchText = string.Empty;
    private Department? _selectedDepartmentFilter;
    private bool _showActiveOnly = true;
    private string _selectedSortProperty = nameof(Employee.LastName);
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;

    // Statistics
    private int _totalEmployees;
    private int _activeEmployees;
    private decimal _averageSalary;
    private int _newHiresThisMonth;

    public EmployeeViewModel(ILogger<EmployeeViewModel> logger, 
        IEmployeeService employeeService, IDepartmentService departmentService,
        IAuditService auditService, INotificationService notificationService) 
        : base(logger)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Initialize collections
        Departments = new ObservableCollection<Department>();
        PositionLevels = new ObservableCollection<string>
        {
            "Junior", "Mid-Level", "Senior", "Lead", "Manager", "Director", "Executive"
        };

        // Initialize commands
        LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployeesAsync, () => !IsBusy);
        SearchCommand = new AsyncRelayCommand(SearchEmployeesAsync, () => !IsBusy);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        AddEmployeeCommand = new RelayCommand(AddEmployee, () => !IsBusy);
        EditEmployeeCommand = new RelayCommand(EditEmployee, () => SelectedEmployee != null && !IsBusy);
        SaveEmployeeCommand = new AsyncRelayCommand(SaveEmployeeAsync, () => !IsBusy && !HasErrors);
        CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditMode);
        DeleteEmployeeCommand = new AsyncRelayCommand(DeleteEmployeeAsync, () => SelectedEmployee != null && !IsBusy);
        RefreshCommand = new AsyncRelayCommand(RefreshDataAsync, () => !IsBusy);
        ExportCommand = new AsyncRelayCommand(ExportEmployeesAsync, () => !IsBusy);
        TransferEmployeeCommand = new AsyncRelayCommand(TransferEmployeeAsync, () => SelectedEmployee != null && !IsBusy);
        ViewEmployeeDetailsCommand = new RelayCommand(ViewEmployeeDetails, () => SelectedEmployee != null);

        _logger.LogInformation("EmployeeViewModel initialized");
    }

    #region Properties

    public ObservableCollection<Department> Departments { get; }
    public ObservableCollection<string> PositionLevels { get; }

    public ICollectionView EmployeesView
    {
        get
        {
            if (_employeesView == null)
            {
                _employeesView = CollectionViewSource.GetDefaultView(_allEmployees);
                _employeesView.Filter = EmployeeFilter;
                _employeesView.SortDescriptions.Add(new SortDescription(_selectedSortProperty, _sortDirection));
            }
            return _employeesView;
        }
    }

    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    public Employee CurrentEmployee
    {
        get => _currentEmployee;
        set => SetProperty(ref _currentEmployee, value);
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
                _employeesView?.Refresh();
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
                _employeesView?.Refresh();
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
                _employeesView?.Refresh();
            }
        }
    }

    public string SelectedSortProperty
    {
        get => _selectedSortProperty;
        set
        {
            if (SetProperty(ref _selectedSortProperty, value))
            {
                ApplySorting();
            }
        }
    }

    public ListSortDirection SortDirection
    {
        get => _sortDirection;
        set
        {
            if (SetProperty(ref _sortDirection, value))
            {
                ApplySorting();
            }
        }
    }

    #endregion

    #region Statistics Properties

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

    public decimal AverageSalary
    {
        get => _averageSalary;
        set => SetProperty(ref _averageSalary, value);
    }

    public int NewHiresThisMonth
    {
        get => _newHiresThisMonth;
        set => SetProperty(ref _newHiresThisMonth, value);
    }

    #endregion

    #region Commands

    public ICommand LoadEmployeesCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand AddEmployeeCommand { get; }
    public ICommand EditEmployeeCommand { get; }
    public ICommand SaveEmployeeCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteEmployeeCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand TransferEmployeeCommand { get; }
    public ICommand ViewEmployeeDetailsCommand { get; }

    #endregion

    #region Initialization

    public override async Task InitializeAsync()
    {
        await LoadDepartmentsAsync();
        await LoadEmployeesAsync();
    }

    public override async Task LoadDataAsync()
    {
        await LoadEmployeesAsync();
    }

    #endregion

    #region Data Loading

    private async Task LoadEmployeesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            
            _allEmployees.Clear();
            foreach (var employee in employees)
            {
                _allEmployees.Add(employee);
            }

            await LoadStatisticsAsync();
            
            _logger.LogInformation("Loaded {Count} employees", _allEmployees.Count);
        }, "Loading employees...");
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

    private async Task LoadStatisticsAsync()
    {
        try
        {
            TotalEmployees = await _employeeService.GetTotalEmployeeCountAsync();
            ActiveEmployees = await _employeeService.GetActiveEmployeeCountAsync();
            AverageSalary = await _employeeService.GetAverageSalaryAsync();
            
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            NewHiresThisMonth = _allEmployees.Count(e => 
                e.HireDate.Month == currentMonth && e.HireDate.Year == currentYear);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load employee statistics");
        }
    }

    #endregion

    #region Search and Filter

    private async Task SearchEmployeesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _employeesView?.Refresh();
            return;
        }

        await ExecuteAsync(async () =>
        {
            var searchResults = await _employeeService.SearchEmployeesAsync(SearchText);
            
            _allEmployees.Clear();
            foreach (var employee in searchResults)
            {
                _allEmployees.Add(employee);
            }
        }, "Searching employees...");
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectedDepartmentFilter = null;
        ShowActiveOnly = true;
        _ = LoadEmployeesAsync();
    }

    private bool EmployeeFilter(object item)
    {
        if (item is not Employee employee)
            return false;

        // Active filter
        if (ShowActiveOnly && !employee.IsActive)
            return false;

        // Department filter
        if (SelectedDepartmentFilter != null && SelectedDepartmentFilter.Id > 0 && 
            employee.DepartmentId != SelectedDepartmentFilter.Id)
            return false;

        // Text search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            return employee.FirstName.ToLowerInvariant().Contains(searchLower) ||
                   employee.LastName.ToLowerInvariant().Contains(searchLower) ||
                   employee.Email.ToLowerInvariant().Contains(searchLower) ||
                   employee.Position.ToLowerInvariant().Contains(searchLower) ||
                   employee.EmployeeNumber.ToLowerInvariant().Contains(searchLower);
        }

        return true;
    }

    private void ApplySorting()
    {
        if (_employeesView != null)
        {
            _employeesView.SortDescriptions.Clear();
            _employeesView.SortDescriptions.Add(new SortDescription(SelectedSortProperty, SortDirection));
        }
    }

    #endregion

    #region CRUD Operations

    private void AddEmployee()
    {
        CurrentEmployee = new Employee
        {
            HireDate = DateTime.UtcNow,
            IsActive = true,
            DepartmentId = Departments.FirstOrDefault(d => d.Id > 0)?.Id ?? 1
        };
        IsEditMode = true;
        SelectedEmployee = null;
    }

    private void EditEmployee()
    {
        if (SelectedEmployee != null)
        {
            // Create a copy for editing
            CurrentEmployee = new Employee
            {
                Id = SelectedEmployee.Id,
                EmployeeNumber = SelectedEmployee.EmployeeNumber,
                FirstName = SelectedEmployee.FirstName,
                LastName = SelectedEmployee.LastName,
                Email = SelectedEmployee.Email,
                PhoneNumber = SelectedEmployee.PhoneNumber,
                Position = SelectedEmployee.Position,
                DepartmentId = SelectedEmployee.DepartmentId,
                Salary = SelectedEmployee.Salary,
                HireDate = SelectedEmployee.HireDate,
                TerminationDate = SelectedEmployee.TerminationDate,
                IsActive = SelectedEmployee.IsActive,
                Address = SelectedEmployee.Address,
                DateOfBirth = SelectedEmployee.DateOfBirth,
                EmergencyContact = SelectedEmployee.EmergencyContact,
                ManagerId = SelectedEmployee.ManagerId
            };
            IsEditMode = true;
        }
    }

    private async Task SaveEmployeeAsync()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                if (CurrentEmployee.Id == 0)
                {
                    // Adding new employee
                    var newEmployee = await _employeeService.CreateEmployeeAsync(CurrentEmployee);
                    _allEmployees.Add(newEmployee);
                    await _notificationService.ShowToastAsync("Success", $"Employee {newEmployee.FullName} has been added.");
                }
                else
                {
                    // Updating existing employee
                    var updatedEmployee = await _employeeService.UpdateEmployeeAsync(CurrentEmployee);
                    var index = _allEmployees.ToList().FindIndex(e => e.Id == updatedEmployee.Id);
                    if (index >= 0)
                    {
                        _allEmployees[index] = updatedEmployee;
                    }
                    await _notificationService.ShowToastAsync("Success", $"Employee {updatedEmployee.FullName} has been updated.");
                }

                IsEditMode = false;
                await LoadStatisticsAsync();
            }
            catch (ArgumentException ex)
            {
                await _notificationService.ShowToastAsync("Validation Error", ex.Message);
            }
        }, "Saving employee...");
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentEmployee = new Employee();
        ClearErrors();
    }

    private async Task DeleteEmployeeAsync()
    {
        if (SelectedEmployee == null) return;

        await ExecuteAsync(async () =>
        {
            var result = await _employeeService.DeleteEmployeeAsync(SelectedEmployee.Id);
            if (result)
            {
                _allEmployees.Remove(SelectedEmployee);
                await _notificationService.ShowToastAsync("Success", $"Employee {SelectedEmployee.FullName} has been deleted.");
                SelectedEmployee = null;
                await LoadStatisticsAsync();
            }
            else
            {
                await _notificationService.ShowToastAsync("Error", "Unable to delete employee. They may have active projects or other dependencies.");
            }
        }, "Deleting employee...");
    }

    #endregion

    #region Additional Operations

    private async Task RefreshDataAsync()
    {
        await LoadDepartmentsAsync();
        await LoadEmployeesAsync();
        await _notificationService.ShowToastAsync("Refreshed", "Employee data has been updated.");
    }

    private async Task ExportEmployeesAsync()
    {
        await ExecuteAsync(async () =>
        {
            // This would typically export to CSV, Excel, or PDF
            _logger.LogInformation("Exporting employee data");
            await _notificationService.ShowToastAsync("Export", "Employee export functionality would be implemented here.");
        }, "Exporting employees...");
    }

    private async Task TransferEmployeeAsync()
    {
        if (SelectedEmployee == null) return;

        await ExecuteAsync(async () =>
        {
            // This would typically open a dialog to select new department
            _logger.LogInformation("Transferring employee {EmployeeId}", SelectedEmployee.Id);
            await _notificationService.ShowToastAsync("Transfer", "Employee transfer dialog would open here.");
        }, "Transferring employee...");
    }

    private void ViewEmployeeDetails()
    {
        if (SelectedEmployee != null)
        {
            _logger.LogInformation("Viewing details for employee {EmployeeId}", SelectedEmployee.Id);
            // This would typically navigate to a detailed employee view
        }
    }

    #endregion

    #region Validation

    protected override void ValidateProperty(string propertyName)
    {
        base.ValidateProperty(propertyName);

        switch (propertyName)
        {
            case nameof(CurrentEmployee.FirstName):
                if (string.IsNullOrWhiteSpace(CurrentEmployee.FirstName))
                    AddError(propertyName, "First name is required.");
                break;

            case nameof(CurrentEmployee.LastName):
                if (string.IsNullOrWhiteSpace(CurrentEmployee.LastName))
                    AddError(propertyName, "Last name is required.");
                break;

            case nameof(CurrentEmployee.Email):
                if (string.IsNullOrWhiteSpace(CurrentEmployee.Email))
                    AddError(propertyName, "Email is required.");
                else if (!IsValidEmail(CurrentEmployee.Email))
                    AddError(propertyName, "Please enter a valid email address.");
                break;

            case nameof(CurrentEmployee.Salary):
                if (CurrentEmployee.Salary < 0)
                    AddError(propertyName, "Salary cannot be negative.");
                break;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
} 