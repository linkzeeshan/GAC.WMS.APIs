using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class ErrorLogRepository : GenericRepository<ErrorLog, int>
    {
        public ErrorLogRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any error log-specific repository methods here
        public async Task<List<ErrorLog>> GetByEntityTypeAndIdAsync(string entityType, string entityId)
        {
            return await _dbSet
                .Where(e => e.EntityType == entityType && e.EntityId == entityId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ErrorLog>> GetByCustomerCodeAsync(string customerCode)
        {
            return await _dbSet
                .Where(e => e.CustomerCode == customerCode)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ErrorLog>> GetUnresolvedErrorsAsync()
        {
            return await _dbSet
                .Where(e => !e.IsResolved)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }
    }
}
