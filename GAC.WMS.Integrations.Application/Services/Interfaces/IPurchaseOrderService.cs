using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;

namespace GAC.WMS.Integrations.Application.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<ApiResponseDto<PurchaseOrderDto>> GetPurchaseOrderByNumberAsync(string poNumber);
        Task<ApiResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(PurchaseOrderDto purchaseOrderDto);
        Task<ApiResponseDto<PurchaseOrderDto>> UpdatePurchaseOrderAsync(string poNumber, PurchaseOrderDto purchaseOrderDto);
        Task<ApiResponseDto<string>> CreatePurchaseOrdersBatchAsync(BatchRequestDto<PurchaseOrderDto> batchRequest);
        Task<ApiResponseDto<List<PurchaseOrderDto>>> GetPurchaseOrderByIdAsync(string customerId);
        Task<ApiResponseDto<List<PurchaseOrderDto>>> GetPurchaseOrderPONumberAsync(string pon);
    }
}
