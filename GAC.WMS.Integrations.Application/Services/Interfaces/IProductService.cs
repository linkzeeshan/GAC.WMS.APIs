using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Products;

namespace GAC.WMS.Integrations.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponseDto<ProductDto>> GetProductByIdAsync(string productId);
        Task<ApiResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto);
        Task<ApiResponseDto<ProductDto>> UpdateProductAsync(string productId, ProductDto productDto);
        Task<ApiResponseDto<string>> CreateProductsBatchAsync(BatchRequestDto<ProductDto> batchRequest);
    }
}
