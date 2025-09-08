using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Repositories
{
    public class FileProcessingJobRepository : GenericRepository<FileProcessingJob, int>
    {
        public FileProcessingJobRepository(DbContext dbContext) : base(dbContext)
        {
        }

        // Add any file processing job-specific repository methods here
        public async Task<List<FileProcessingJob>> GetPendingJobsAsync()
        {
            return await _dbSet
                .Where(j => j.Status == "Pending")
                .OrderBy(j => j.ScheduledTime)
                .ToListAsync();
        }

        public async Task<List<FileProcessingJob>> GetJobsByStatusAsync(string status)
        {
            return await _dbSet
                .Where(j => j.Status == status)
                .OrderByDescending(j => j.ScheduledTime)
                .ToListAsync();
        }

        public async Task<List<FileProcessingJob>> GetJobsForRetryAsync()
        {
            return await _dbSet
                .Where(j => j.Status == "Failed" && j.RetryCount < 3 && j.NextRetryTime <= DateTime.UtcNow)
                .OrderBy(j => j.NextRetryTime)
                .ToListAsync();
        }

        public async Task<List<FileProcessingJob>> GetJobsByCustomerCodeAsync(string customerCode)
        {
            return await _dbSet
                .Where(j => j.CustomerCode == customerCode)
                .OrderByDescending(j => j.ScheduledTime)
                .ToListAsync();
        }
    }
}
