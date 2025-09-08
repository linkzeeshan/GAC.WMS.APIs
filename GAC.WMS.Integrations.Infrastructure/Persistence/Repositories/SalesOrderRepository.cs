using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class SalesOrderRepository : GenericRepository<SalesOrder, int>
    {
        public SalesOrderRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any sales order-specific repository methods here
        public async Task<SalesOrder?> GetBySONumberAsync(string soNumber)
        {
            return await _dbSet
                .Include(so => so.CustomerEntity)
                .Include(so => so.SOLines)
                    .ThenInclude(sol => sol.Product)
                .FirstOrDefaultAsync(so => so.SONumber == soNumber);
        }

        public async Task<bool> ExistsBySONumberAsync(string soNumber)
        {
            return await _dbSet.AnyAsync(so => so.SONumber == soNumber);
        }

        public async Task<List<SalesOrder>> GetByCustomerIdAsync(int customerEntityId)
        {
            return await _dbSet
                .Include(so => so.SOLines)
                .Where(so => so.CustomerEntityId == customerEntityId)
                .ToListAsync();
        }

        public async Task<List<SalesOrder>> GetByCustomerCodeAsync(string customerId)
        {
            return await _dbSet
                .Include(so => so.SOLines)
                .Where(so => so.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
