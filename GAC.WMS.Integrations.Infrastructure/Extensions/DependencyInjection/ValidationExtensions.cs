using FluentValidation;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Register individual validators
            services.AddScoped<IValidator<CustomerDto>, CustomerDtoValidator>();
            services.AddScoped<IValidator<ProductDto>, ProductDtoValidator>();
            services.AddScoped<IValidator<PurchaseOrderDto>, PurchaseOrderDtoValidator>();
            services.AddScoped<IValidator<SalesOrderDto>, SalesOrderDtoValidator>();
            services.AddScoped<IValidator<PurchaseOrderLineDto>, PurchaseOrderLineDtoValidator>();
            services.AddScoped<IValidator<SalesOrderLineDto>, SalesOrderLineDtoValidator>();
            
            // Register batch validators
            services.AddScoped<IValidator<BatchRequestDto<CustomerDto>>>(provider => 
                new BatchRequestDtoValidator<CustomerDto>(provider.GetRequiredService<IValidator<CustomerDto>>()));
            
            services.AddScoped<IValidator<BatchRequestDto<ProductDto>>>(provider => 
                new BatchRequestDtoValidator<ProductDto>(provider.GetRequiredService<IValidator<ProductDto>>()));
            
            services.AddScoped<IValidator<BatchRequestDto<PurchaseOrderDto>>>(provider => 
                new BatchRequestDtoValidator<PurchaseOrderDto>(provider.GetRequiredService<IValidator<PurchaseOrderDto>>()));
            
            services.AddScoped<IValidator<BatchRequestDto<SalesOrderDto>>>(provider => 
                new BatchRequestDtoValidator<SalesOrderDto>(provider.GetRequiredService<IValidator<SalesOrderDto>>()));

            return services;
        }
    }
}
