using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;

namespace GAC.WMS.Integrations.Application.Services.Interfaces
{
    public interface ISalesOrderService
    {
        Task<ApiResponseDto<SalesOrderDto>> GetSalesOrderByNumberAsync(string soNumber);
        Task<ApiResponseDto<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderDto salesOrderDto);
        Task<ApiResponseDto<SalesOrderDto>> UpdateSalesOrderAsync(string soNumber, SalesOrderDto salesOrderDto);
        Task<ApiResponseDto<string>> CreateSalesOrdersBatchAsync(BatchRequestDto<SalesOrderDto> batchRequest);
        Task<ApiResponseDto<List<SalesOrderDto>>> GetSalesOrdersByCustomerIdAsync(string customerId);
    }
}
