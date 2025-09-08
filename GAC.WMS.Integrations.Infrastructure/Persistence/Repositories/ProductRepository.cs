using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : GenericRepository<Product, int>
    {
        public ProductRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any product-specific repository methods here
        public async Task<Product?> GetByProductIdAsync(string productId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
        }

        public async Task<bool> ExistsByProductIdAsync(string productId)
        {
            return await _dbSet.AnyAsync(p => p.ProductId == productId);
        }
    }
}
