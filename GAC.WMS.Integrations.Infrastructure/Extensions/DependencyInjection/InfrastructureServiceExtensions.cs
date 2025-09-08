using GAC.WMS.Integrations.Application.Services.Communication;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Interfaces;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GAC.WMS.Integrations.Infrastructure.Extensions.DependencyInjection
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register DbContext as DbContext for generic repositories
            services.AddScoped<DbContext, ApplicationDbContext>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register WMS API client
            services.AddHttpClient<IWmsApiClient, WmsApiClient>();

            // Register Quartz jobs
            services.AddQuartzJobs(configuration);

            return services;
        }
    }
}
