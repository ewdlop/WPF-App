using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace WpfApp2.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = context.Set<T>();
    }

    #region Basic CRUD Operations

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID: {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding {EntityType} with predicate", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first {EntityType} with predicate", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.AnyAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if any {EntityType} exists with predicate", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            return predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Add Operations

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            var result = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added new {EntityType}", typeof(T).Name);
            return result.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        try
        {
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            return entityList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding range of {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Update Operations

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated {EntityType}", typeof(T).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        try
        {
            var entityList = entities.ToList();
            _dbSet.UpdateRange(entityList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            return entityList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating range of {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Delete Operations

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} not found for deletion", typeof(T).Name, id);
                return false;
            }

            return await DeleteAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} by ID: {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted {EntityType}", typeof(T).Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
    {
        try
        {
            var entityList = entities.ToList();
            _dbSet.RemoveRange(entityList);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting range of {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Pagination

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        try
        {
            IQueryable<T> query = _dbSet;

            // Apply filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Include properties
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Apply pagination
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Advanced Querying

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public virtual async Task<IEnumerable<T>> GetWithIncludeAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string includeProperties = "")
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with includes", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Bulk Operations

    public virtual async Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk deleted {Count} {EntityType} entities", entities.Count, typeof(T).Name);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<int> BulkUpdateAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression)
    {
        try
        {
            // Note: This is a simplified implementation. For production, consider using libraries like EF Extensions
            var entities = await _dbSet.Where(predicate).ToListAsync();
            var updateFunc = updateExpression.Compile();
            
            foreach (var entity in entities)
            {
                var updatedEntity = updateFunc(entity);
                _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk updated {Count} {EntityType} entities", entities.Count, typeof(T).Name);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating {EntityType}", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Existence Checks

    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType} with ID: {Id}", typeof(T).Name, id);
            throw;
        }
    }

    #endregion
} 