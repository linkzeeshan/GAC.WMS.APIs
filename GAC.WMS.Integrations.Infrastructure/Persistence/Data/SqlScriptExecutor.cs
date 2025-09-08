using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    public class SqlScriptExecutor
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SqlScriptExecutor> _logger;

        public SqlScriptExecutor(
            ApplicationDbContext dbContext,
            ILogger<SqlScriptExecutor> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task ExecuteScriptAsync(string scriptPath)
        {
            _logger.LogInformation("Executing SQL script: {ScriptPath}", scriptPath);

            try
            {
                // Read the script file
                string script = await File.ReadAllTextAsync(scriptPath);

                // Split the script by GO statements if present
                string[] commandTexts = script.Split(new[] { "GO", "go" }, StringSplitOptions.RemoveEmptyEntries);

                // Get the connection from the DbContext
                var connection = _dbContext.Database.GetDbConnection();
                var connectionState = connection.State;

                try
                {
                    if (connectionState != System.Data.ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }

                    // Execute each command
                    foreach (string commandText in commandTexts)
                    {
                        if (!string.IsNullOrWhiteSpace(commandText))
                        {
                            using var command = connection.CreateCommand();
                            command.CommandText = commandText;
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    _logger.LogInformation("SQL script executed successfully");
                }
                finally
                {
                    // Only close the connection if we opened it
                    if (connectionState != System.Data.ConnectionState.Open && connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL script: {ScriptPath}", scriptPath);
                throw;
            }
        }
    }
}
