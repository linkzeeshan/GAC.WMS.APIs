namespace GAC.WMS.Integrations.Infrastructure.Configurations
{
    public class SchedulerConfig
    {
        public string PollingInterval { get; set; } = "0 */5 * * * ?"; // Default: Every 5 minutes
        public List<JobConfig> Jobs { get; set; } = new List<JobConfig>();
    }

    public class JobConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // SFTP, FileSystem, etc.
        public string CronExpression { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
