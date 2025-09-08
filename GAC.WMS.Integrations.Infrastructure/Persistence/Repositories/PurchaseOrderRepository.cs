using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class PurchaseOrderRepository : GenericRepository<PurchaseOrder, int>
    {
        public PurchaseOrderRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any purchase order-specific repository methods here
        public async Task<PurchaseOrder?> GetByPONumberAsync(string poNumber)
        {
            return await _dbSet
                .Include(po => po.Customer)
                .Include(po => po.POLines)
                    .ThenInclude(pol => pol.Product)
                .FirstOrDefaultAsync(po => po.PONumber == poNumber);
        }

        public async Task<bool> ExistsByPONumberAsync(string poNumber)
        {
            return await _dbSet.AnyAsync(po => po.PONumber == poNumber);
        }

        public async Task<List<PurchaseOrder>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(po => po.POLines)
                .Where(po => po.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
