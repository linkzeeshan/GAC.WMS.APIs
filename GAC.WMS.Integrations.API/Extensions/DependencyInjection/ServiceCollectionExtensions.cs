using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GAC.WMS.Integrations.API.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register validators from the API assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            return services;
        }
    }
}
