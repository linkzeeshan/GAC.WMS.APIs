using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    public class DatabaseMigrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseMigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration service");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                _logger.LogInformation("Checking database existence and applying migrations if needed");
                
                // Ensure database exists
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                
                // Apply any pending migrations
                if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                {
                    _logger.LogInformation("Applying pending migrations");
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    _logger.LogInformation("No pending migrations found");
                }
                
                // Verify all required tables exist
                await VerifyTablesExistAsync(dbContext, cancellationToken);
                
                // Execute SQL script to add audit columns
                await AddAuditColumnsAsync(dbContext, cancellationToken);
                
                _logger.LogInformation("Database migration service completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while migrating the database");
                // Don't rethrow - we don't want to prevent the application from starting
                // just because migrations failed
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        private async Task VerifyTablesExistAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verifying required tables exist");
            
            // Get all entity types from the model
            var entityTypes = dbContext.Model.GetEntityTypes().ToList();
            var tableNames = entityTypes.Select(e => e.GetTableName()).ToList();
            
            _logger.LogInformation("Expected tables: {TableNames}", string.Join(", ", tableNames));
            
            // Check if tables exist in the database
            var existingTables = await GetExistingTablesAsync(dbContext, cancellationToken);
            _logger.LogInformation("Existing tables: {TableNames}", string.Join(", ", existingTables));
            
            // Find missing tables
            var missingTables = tableNames.Except(existingTables, StringComparer.OrdinalIgnoreCase).ToList();
            
            if (missingTables.Any())
            {
                _logger.LogWarning("Missing tables detected: {MissingTables}", string.Join(", ", missingTables));
                
                // Force a new migration if tables are missing
                _logger.LogInformation("Attempting to recreate missing tables by applying migrations");
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("All required tables exist");
            }
        }
        
        private async Task AddAuditColumnsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding audit columns to tables if missing");
            
            try
            {
                // Check if we need to add audit columns by checking if the Customers table has CreatedBy column
                var needsAuditColumns = await CheckIfAuditColumnsNeededAsync(dbContext, cancellationToken);
                
                if (needsAuditColumns)
                {
                    _logger.LogInformation("Audit columns are missing, adding them now");
                    
                    // Execute SQL directly to add the columns
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State != ConnectionState.Open)
                    {
                        await connection.OpenAsync(cancellationToken);
                    }
                    
                    try
                    {
                        // Add CreatedBy column to all tables that inherit from AuditableEntity
                        var tables = new[] { "Customers", "Products", "PurchaseOrders", "PurchaseOrderLines", 
                                          "SalesOrders", "SalesOrderLines", "FileProcessingJobs", 
                                          "IntegrationLogs", "ErrorLogs", "IntegrationMessages" };
                        
                        foreach (var table in tables)
                        {
                            try
                            {
                                using var command = connection.CreateCommand();
                                command.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.{table}') AND name = 'CreatedBy') " +
                                                     $"ALTER TABLE dbo.{table} ADD CreatedBy NVARCHAR(256) NULL";
                                await command.ExecuteNonQueryAsync(cancellationToken);
                                
                                command.CommandText = $"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.{table}') AND name = 'LastModifiedBy') " +
                                                     $"ALTER TABLE dbo.{table} ADD LastModifiedBy NVARCHAR(256) NULL";
                                await command.ExecuteNonQueryAsync(cancellationToken);
                                
                                _logger.LogInformation("Added audit columns to {Table} table", table);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error adding audit columns to {Table} table", table);
                                // Continue with other tables
                            }
                        }
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Audit columns already exist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding audit columns");
                // Don't rethrow - we don't want to prevent the application from starting
            }
        }
        
        private async Task<bool> CheckIfAuditColumnsNeededAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }
            
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(1) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Customers') AND name = 'CreatedBy'";
                var result = await command.ExecuteScalarAsync(cancellationToken);
                
                // If count is 0, we need to add the columns
                return Convert.ToInt32(result) == 0;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
        
        private async Task<List<string>> GetExistingTablesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var tables = new List<string>();
            
            // Get database connection
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }
            
            try
            {
                // Get schema table
                DataTable schema;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    using var reader = await command.ExecuteReaderAsync(cancellationToken);
                    schema = new DataTable();
                    schema.Load(reader);
                }
                
                // Extract table names
                foreach (DataRow row in schema.Rows)
                {
                    var tableName = row["TABLE_NAME"].ToString();
                    if (!string.IsNullOrEmpty(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
            
            return tables;
        }
    }
}
