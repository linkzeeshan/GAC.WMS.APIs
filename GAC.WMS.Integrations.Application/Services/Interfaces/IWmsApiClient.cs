using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;

namespace GAC.WMS.Integrations.Application.Services.Interfaces
{
    public interface IWmsApiClient
    {
        // Strongly-typed DTO methods
        Task<ApiResponseDto<CustomerDto>> CreateCustomerAsync(CustomerDto customer);
        Task<ApiResponseDto<string>> CreateCustomersBatchAsync(BatchRequestDto<CustomerDto> batchRequest);
        
        Task<ApiResponseDto<ProductDto>> CreateProductAsync(ProductDto product);
        Task<ApiResponseDto<string>> CreateProductsBatchAsync(BatchRequestDto<ProductDto> batchRequest);
        
        Task<ApiResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(PurchaseOrderDto purchaseOrder);
        Task<ApiResponseDto<string>> CreatePurchaseOrdersBatchAsync(BatchRequestDto<PurchaseOrderDto> batchRequest);
        
        Task<ApiResponseDto<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderDto salesOrder);
        Task<ApiResponseDto<string>> CreateSalesOrdersBatchAsync(BatchRequestDto<SalesOrderDto> batchRequest);
        
        // Raw payload methods for outbox processing
        Task<ApiResponseDto<object>> CreatePurchaseOrderAsync(string rawPayload);
        Task<ApiResponseDto<object>> CreateSalesOrderAsync(string rawPayload);
        Task<ApiResponseDto<object>> CreateCustomerAsync(string rawPayload);
        Task<ApiResponseDto<object>> CreateProductAsync(string rawPayload);
    }
}
