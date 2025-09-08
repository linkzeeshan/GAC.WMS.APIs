using GAC.WMS.Integrations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public DbSet<FileProcessingJob> FileProcessingJobs { get; set; }
        public DbSet<IntegrationLog> IntegrationLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<IntegrationMessage> IntegrationMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Suppress the pending model changes warning
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            
            base.OnConfiguring(optionsBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get current user
            string? currentUser = "system"; // Default value
            
            // Update audit fields
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = currentUser;
                        break;
                }
            }
            
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Customer)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(po => po.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(so => so.CustomerEntity)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(so => so.CustomerEntityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PurchaseOrderLine>()
                .HasOne(pol => pol.PurchaseOrder)
                .WithMany(po => po.POLines)
                .HasForeignKey(pol => pol.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderLine>()
                .HasOne(pol => pol.Product)
                .WithMany(p => p.PurchaseOrderLines)
                .HasForeignKey(pol => pol.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(sol => sol.SalesOrder)
                .WithMany(so => so.SOLines)
                .HasForeignKey(sol => sol.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(sol => sol.Product)
                .WithMany(p => p.SalesOrderLines)
                .HasForeignKey(sol => sol.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
