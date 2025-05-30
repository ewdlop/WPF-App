using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace WpfApp2.ViewModels.Base;

public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    protected readonly ILogger _logger;
    private bool _isBusy;
    private string _busyMessage = "Loading...";
    private bool _disposed;

    protected BaseViewModel(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            onChanged?.Invoke();
            return true;
        }
        return false;
    }

    #endregion

    #region Busy State Management

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    protected async Task ExecuteAsync(Func<Task> operation, string? busyMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (!string.IsNullOrEmpty(busyMessage))
                BusyMessage = busyMessage;

            await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing operation: {Message}", ex.Message);
            await HandleErrorAsync(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? busyMessage = null)
    {
        if (IsBusy) return default;

        try
        {
            IsBusy = true;
            if (!string.IsNullOrEmpty(busyMessage))
                BusyMessage = busyMessage;

            return await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing operation: {Message}", ex.Message);
            await HandleErrorAsync(ex);
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Error Handling

    protected virtual async Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = $"An error occurred: {exception.Message}";
        _logger.LogError(exception, errorMessage);
        
        // In a real application, you might show a message box or notification
        // For now, we'll just log the error
        await Task.CompletedTask;
    }

    #endregion

    #region Validation Support

    private readonly Dictionary<string, List<string>> _validationErrors = new();

    public bool HasErrors => _validationErrors.Count > 0;

    public IEnumerable<string> GetErrors(string propertyName)
    {
        return _validationErrors.ContainsKey(propertyName) 
            ? _validationErrors[propertyName] 
            : Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetAllErrors()
    {
        return _validationErrors.Values.SelectMany(errors => errors);
    }

    protected void SetError(string propertyName, string error)
    {
        if (!_validationErrors.ContainsKey(propertyName))
            _validationErrors[propertyName] = new List<string>();

        if (!_validationErrors[propertyName].Contains(error))
        {
            _validationErrors[propertyName].Add(error);
            OnPropertyChanged(nameof(HasErrors));
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_validationErrors.ContainsKey(propertyName))
        {
            _validationErrors.Remove(propertyName);
            OnPropertyChanged(nameof(HasErrors));
        }
    }

    protected void ClearAllErrors()
    {
        _validationErrors.Clear();
        OnPropertyChanged(nameof(HasErrors));
    }

    protected void AddError(string propertyName, string error)
    {
        SetError(propertyName, error);
    }

    protected virtual void ValidateProperty(string propertyName)
    {
        // Override in derived classes for property-specific validation
        // This method is called when properties change and need validation
    }

    protected void SetErrorMessage(string message)
    {
        _logger.LogError(message);
        // You might want to set a general error message property here
    }

    protected void ClearErrors()
    {
        ClearAllErrors();
    }

    #endregion

    #region Lifecycle Methods

    public virtual async Task InitializeAsync()
    {
        // Override in derived classes for initialization logic
        await Task.CompletedTask;
    }

    public virtual async Task LoadDataAsync()
    {
        // Override in derived classes for data loading logic
        await Task.CompletedTask;
    }

    public virtual async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Dispose managed resources
            _disposed = true;
        }
    }

    #endregion
} 