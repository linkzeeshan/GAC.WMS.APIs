using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.Services.Implementation;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GAC.WMS.Integrations.Application.Extensions.DependencyInjection
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register application services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<ISalesOrderService, SalesOrderService>();
            // Register validators
            services.AddValidatorsFromAssemblyContaining<CustomerDtoValidator>();
            
            // Register services
            // Add your service registrations here
            
            return services;
        }
    }
}
