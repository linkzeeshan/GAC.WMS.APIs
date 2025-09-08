using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : GenericRepository<Customer, int>
    {
        public CustomerRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any customer-specific repository methods here
        public async Task<Customer?> GetByCustomerIdAsync(string customerId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<bool> ExistsByCustomerIdAsync(string customerId)
        {
            return await _dbSet.AnyAsync(c => c.CustomerId == customerId);
        }
    }
}
