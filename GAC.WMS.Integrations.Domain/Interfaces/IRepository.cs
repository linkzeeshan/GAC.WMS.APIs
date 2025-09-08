using System.Linq.Expressions;

namespace GAC.WMS.Integrations.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public interface IRepository<T, TKey> where T : class, IEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>IQueryable of entities</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Gets entities based on a predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <returns>IQueryable of filtered entities</returns>
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets an entity by its primary key
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <returns>Entity or null if not found</returns>
        Task<T?> GetByIdAsync(TKey id);

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">Entity to create</param>
        Task CreateAsync(T entity);

        /// <summary>
        /// Creates multiple entities
        /// </summary>
        /// <param name="entities">Entities to create</param>
        Task CreateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        void Update(T entity);

        /// <summary>
        /// Updates multiple entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        void Delete(T entity);

        /// <summary>
        /// Deletes multiple entities
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        void DeleteRange(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity by its primary key
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <returns>True if entity was deleted, false if not found</returns>
        Task<bool> DeleteByIdAsync(TKey id);
    }
}
