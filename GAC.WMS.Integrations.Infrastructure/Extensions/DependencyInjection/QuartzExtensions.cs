using GAC.WMS.Integrations.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GAC.WMS.Integrations.Infrastructure.Extensions.DependencyInjection
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Quartz
            services.AddQuartz(q =>
            {
                // Configure jobs
                
                // XML Import Job
                var xmlImportJobKey = new JobKey("XmlImportJob");
                q.AddJob<XmlImportJob>(opts => opts.WithIdentity(xmlImportJobKey));
                
                q.AddTrigger(opts => opts
                    .ForJob(xmlImportJobKey)
                    .WithIdentity("XmlImportJob-Trigger")
                    .WithCronSchedule(configuration["Quartz:XmlImportJob:CronSchedule"] ?? "0 */5 * * * ?"));
                
                // Outbox Processor Job
                var outboxProcessorJobKey = new JobKey("OutboxProcessorJob");
                q.AddJob<OutboxProcessorJob>(opts => opts.WithIdentity(outboxProcessorJobKey));
                
                q.AddTrigger(opts => opts
                    .ForJob(outboxProcessorJobKey)
                    .WithIdentity("OutboxProcessorJob-Trigger")
                    .WithCronSchedule(configuration["Quartz:OutboxProcessorJob:CronSchedule"] ?? "0 */2 * * * ?"));
                
                // Configure Quartz settings
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
            });
            
            // Add Quartz hosted service
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
            
            return services;
        }
    }
}
