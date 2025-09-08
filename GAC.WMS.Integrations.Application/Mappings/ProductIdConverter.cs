using AutoMapper;
using GAC.WMS.Integrations.Domain.Entities;

namespace GAC.WMS.Integrations.Application.Mappings
{
    public class ProductIdConverter : IValueResolver<PurchaseOrderLine, DTOs.PurchaseOrders.PurchaseOrderLineDto, int>
    {
        public int Resolve(PurchaseOrderLine source, DTOs.PurchaseOrders.PurchaseOrderLineDto destination, int destMember, ResolutionContext context)
        {
            // If Product is null, return the ProductId from the PurchaseOrderLine
            if (source.Product == null)
            {
                return source.ProductId;
            }

            // Try to parse the string ProductId to int
            if (int.TryParse(source.Product.ProductId, out int productId))
            {
                return productId;
            }

            // If parsing fails, return the ProductId from the PurchaseOrderLine
            return source.ProductId;
        }
    }
}
