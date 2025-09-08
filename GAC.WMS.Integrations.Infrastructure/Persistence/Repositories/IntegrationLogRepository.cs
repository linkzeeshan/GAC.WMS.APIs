using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class IntegrationLogRepository : GenericRepository<IntegrationLog, int>
    {
        public IntegrationLogRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any integration log-specific repository methods here
        public async Task<List<IntegrationLog>> GetByEntityTypeAndIdAsync(string entityType, string entityId)
        {
            return await _dbSet
                .Where(i => i.EntityType == entityType && i.EntityId == entityId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<IntegrationLog>> GetByCustomerCodeAsync(string customerCode)
        {
            return await _dbSet
                .Where(i => i.CustomerCode == customerCode)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<IntegrationLog>> GetByIntegrationTypeAsync(string integrationType)
        {
            return await _dbSet
                .Where(i => i.IntegrationType == integrationType)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }
    }
}
