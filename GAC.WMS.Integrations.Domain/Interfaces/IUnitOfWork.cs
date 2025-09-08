namespace GAC.WMS.Integrations.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work interface to manage transactions across multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets a repository for a specific entity type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TKey">Primary key type</typeparam>
        /// <returns>Repository for the entity type</returns>
        IRepository<T, TKey> GetRepository<T, TKey>() where T : class, IEntity<TKey> where TKey : IEquatable<TKey>;

        /// <summary>
        /// Saves all changes made in this unit of work to the database
        /// </summary>
        /// <returns>Number of state entries written to the database</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Begins a transaction
        /// </summary>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
