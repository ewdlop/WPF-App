using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using WpfApp2.Data.Repositories;

namespace WpfApp2.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    // Repository backing fields
    private IEmployeeRepository? _employees;
    private IProjectRepository? _projects;
    private IDepartmentRepository? _departments;
    private IRepository<Models.AuditLog>? _auditLogs;
    private IRepository<Models.ProjectAssignment>? _projectAssignments;
    private IRepository<Models.ProjectMilestone>? _projectMilestones;


    public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repositories = new Dictionary<Type, object>();
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    #region Repository Properties

    public IEmployeeRepository Employees
    {
        get
        {
            _employees ??= new EmployeeRepository(_context,
                _loggerFactory.CreateLogger<EmployeeRepository>());
            return _employees;
        }
    }

    public IProjectRepository Projects
    {
        get
        {
            _projects ??= new ProjectRepository(_context,
                _loggerFactory.CreateLogger<ProjectRepository>());
            return _projects;
        }
    }

    public IDepartmentRepository Departments
    {
        get
        {
            _departments ??= new DepartmentRepository(_context,
                _loggerFactory.CreateLogger<DepartmentRepository>());
            return _departments;
        }
    }

    public IRepository<Models.AuditLog> AuditLogs
    {
        get
        {
            _auditLogs ??= new Repository<Models.AuditLog>(_context,
                _loggerFactory.CreateLogger<Repository<Models.AuditLog>>());
            return _auditLogs;
        }
    }

    public IRepository<Models.ProjectAssignment> ProjectAssignments
    {
        get
        {
            _projectAssignments ??= new Repository<Models.ProjectAssignment>(_context,
                _loggerFactory.CreateLogger<Repository<Models.ProjectAssignment>>());
            return _projectAssignments;
        }
    }

    public IRepository<Models.ProjectMilestone> ProjectMilestones
    {
        get
        {
            _projectMilestones ??= new Repository<Models.ProjectMilestone>(_context,
                _loggerFactory.CreateLogger<Repository<Models.ProjectMilestone>>());
            return _projectMilestones;
        }
    }

    #endregion

    #region Generic Repository Access

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (_repositories.ContainsKey(type))
        {
            return (IRepository<T>)_repositories[type];
        }

        var repository = new Repository<T>(_context, _loggerFactory.CreateLogger<Repository<T>>());
        _repositories[type] = repository;
        
        return repository;
    }

    #endregion

    #region Transaction Management

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public int SaveChanges()
    {
        try
        {
            var result = _context.SaveChanges();
            _logger.LogInformation("Saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Database transaction started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting database transaction");
            throw;
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            await SaveChangesAsync();
            await _currentTransaction.CommitAsync();
            
            _logger.LogInformation("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing database transaction");
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("Attempted to rollback but no transaction in progress");
                return;
            }

            await _currentTransaction.RollbackAsync();
            _logger.LogInformation("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back database transaction");
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    #endregion

    #region Dispose Pattern

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Dispose the current transaction if it exists
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            // Dispose the context
            _context?.Dispose();
            
            _disposed = true;
            _logger.LogInformation("UnitOfWork disposed");
        }
    }

    #endregion
} 