using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Domain.Entities;
using System.Text.Json;

namespace GAC.WMS.Integrations.Infrastructure.Persistence.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer mappings
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrders, opt => opt.Ignore());

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Attributes) 
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Attributes, new JsonSerializerOptions()) 
                        : null));
            
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PurchaseOrderLines, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderLines, opt => opt.Ignore())
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => 
                    src.Attributes != null 
                        ? JsonSerializer.Serialize(src.Attributes, new JsonSerializerOptions()) 
                        : null));

            // Purchase Order mappings
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.POLines, opt => opt.MapFrom(src => src.POLines))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerId : null));
            
            CreateMap<PurchaseOrderDto, PurchaseOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.POLines, opt => opt.Ignore());

            // Purchase Order Line mappings
            CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId));
            
            CreateMap<PurchaseOrderLineDto, PurchaseOrderLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore());

            // Sales Order mappings
            CreateMap<SalesOrder, SalesOrderDto>()
                .ForMember(dest => dest.SOLines, opt => opt.MapFrom(src => src.SOLines));
            
            CreateMap<SalesOrderDto, SalesOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CustomerEntity, opt => opt.Ignore())
                .ForMember(dest => dest.SOLines, opt => opt.Ignore());

            // Sales Order Line mappings
            CreateMap<SalesOrderLine, SalesOrderLineDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId));
            
            CreateMap<SalesOrderLineDto, SalesOrderLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore());
        }
    }
}
