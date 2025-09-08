using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    /// <summary>
    /// Generic repository implementation for CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public class GenericRepository<T, TKey> : IRepository<T, TKey> 
        where T : class, IEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        /// <inheritdoc />
        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        /// <inheritdoc />
        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <inheritdoc />
        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <inheritdoc />
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        /// <inheritdoc />
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <inheritdoc />
        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                return false;
            }

            Delete(entity);
            return true;
        }
    }
}
