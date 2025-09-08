using AutoMapper;
using GAC.WMS.Integrations.Domain.Entities;

namespace GAC.WMS.Integrations.Application.Mappings
{
    public class SalesOrderProductIdConverter : IValueResolver<SalesOrderLine, DTOs.SalesOrders.SalesOrderLineDto, int>
    {
        public int Resolve(SalesOrderLine source, DTOs.SalesOrders.SalesOrderLineDto destination, int destMember, ResolutionContext context)
        {
            // Return the ProductId from the SalesOrderLine
            return source.ProductId;
        }
    }
}
