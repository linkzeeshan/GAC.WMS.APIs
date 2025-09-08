using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using GAC.WMS.Integrations.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    /// <summary>
    /// Implementation of the Unit of Work pattern to manage transactions across multiple repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Specific repositories
        private CustomerRepository? _customerRepository;
        private ProductRepository? _productRepository;
        private PurchaseOrderRepository? _purchaseOrderRepository;
        private SalesOrderRepository? _salesOrderRepository;
        private ErrorLogRepository? _errorLogRepository;
        private IntegrationLogRepository? _integrationLogRepository;
        private FileProcessingJobRepository? _fileProcessingJobRepository;

        // Repository properties
        public CustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository(_dbContext);
        public ProductRepository ProductRepository => _productRepository ??= new ProductRepository(_dbContext);
        public PurchaseOrderRepository PurchaseOrderRepository => _purchaseOrderRepository ??= new PurchaseOrderRepository(_dbContext);
        public SalesOrderRepository SalesOrderRepository => _salesOrderRepository ??= new SalesOrderRepository(_dbContext);
        public ErrorLogRepository ErrorLogRepository => _errorLogRepository ??= new ErrorLogRepository(_dbContext);
        public IntegrationLogRepository IntegrationLogRepository => _integrationLogRepository ??= new IntegrationLogRepository(_dbContext);
        public FileProcessingJobRepository FileProcessingJobRepository => _fileProcessingJobRepository ??= new FileProcessingJobRepository(_dbContext);

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        /// <inheritdoc />
        public IRepository<T, TKey> GetRepository<T, TKey>() where T : class, IEntity<TKey> where TKey : IEquatable<TKey>
        {
            return (IRepository<T, TKey>)_repositories.GetOrAdd(
                typeof(T), 
                _ => new GenericRepository<T, TKey>(_dbContext));
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Disposes the context and transaction
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the context and transaction
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _dbContext.Dispose();
                _disposed = true;
            }
        }
    }
}
