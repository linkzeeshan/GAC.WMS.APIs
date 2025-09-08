using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Domain.Entities;

namespace GAC.WMS.Integrations.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer mappings
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ReverseMap();

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.GetAttributes()))
                .ReverseMap()
                .ForMember(dest => dest.Attributes, opt => opt.Ignore());

            // PurchaseOrder mappings
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.POLines, opt => opt.MapFrom(src => src.POLines))
                .ReverseMap()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.POLines, opt => opt.Ignore()); // Ignore POLines when mapping from DTO to entity

            // PurchaseOrderLine mappings
            CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>()
                // Use custom converter to handle ProductId mapping
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom<ProductIdConverter>())
                .ReverseMap()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore());

            // SalesOrder mappings
            CreateMap<SalesOrder, SalesOrderDto>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerEntity != null ? src.CustomerEntity.CustomerId : src.CustomerId))
                .ForMember(dest => dest.SOLines, opt => opt.MapFrom(src => src.SOLines))
                .ReverseMap()
                .ForMember(dest => dest.CustomerEntityId, opt => opt.Ignore()) // Ignore CustomerEntityId in mapping to prevent conflicts
                .ForMember(dest => dest.CustomerEntity, opt => opt.Ignore())
                .ForMember(dest => dest.SOLines, opt => opt.Ignore()); // Ignore SOLines when mapping from DTO to entity

            // SalesOrderLine mappings
            CreateMap<SalesOrderLine, SalesOrderLineDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom<SalesOrderProductIdConverter>())
                .ReverseMap()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrder, opt => opt.Ignore());
        }
    }
}
