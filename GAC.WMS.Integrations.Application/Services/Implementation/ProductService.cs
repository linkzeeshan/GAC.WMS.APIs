using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GAC.WMS.Integrations.Application.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponseDto<ProductDto>> GetProductByIdAsync(string productId)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", productId);
            
            var productRepository = _unitOfWork.GetRepository<Product, int>();
            var product = await productRepository.GetByCondition(p => p.ProductId == productId).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return ApiResponseDto<ProductDto>.ErrorResponse($"Product with ID {productId} not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return ApiResponseDto<ProductDto>.SuccessResponse(productDto);
        }

        public async Task<ApiResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto)
        {
            // Ensure ProductId is set (will use the default if not provided)
            if (string.IsNullOrEmpty(productDto.ProductId))
            {
                productDto.ProductId = Guid.CreateVersion7().ToString();
            }
            
            _logger.LogInformation("Creating new product with ID: {ProductId}", productDto.ProductId);
            
            var productRepository = _unitOfWork.GetRepository<Product, int>();
            
            // Check if product already exists
            var existingProduct = await productRepository.GetByCondition(p => p.ProductId == productDto.ProductId).FirstOrDefaultAsync();
            if (existingProduct != null)
            {
                _logger.LogWarning("Product already exists: {ProductId}", productDto.ProductId);
                return ApiResponseDto<ProductDto>.ErrorResponse($"Product with ID {productDto.ProductId} already exists");
            }

            try
            {
                var product = _mapper.Map<Product>(productDto);
                
                // Handle attributes dictionary
                if (productDto.Attributes != null && productDto.Attributes.Count > 0)
                {
                    product.SetAttributes(productDto.Attributes);
                }
                
                await productRepository.CreateAsync(product);
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Product",
                    EntityId = product.ProductId,
                    Status = "Success",
                    Message = "Product created successfully"
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product created successfully: {ProductId}", productDto.ProductId);
                return ApiResponseDto<ProductDto>.SuccessResponse(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductId}", productDto.ProductId);
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "ProductService.CreateProduct",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "Product",
                    EntityId = productDto.ProductId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<ProductDto>.ErrorResponse("Error creating product", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<ProductDto>> UpdateProductAsync(string productId, ProductDto productDto)
        {
            // Force the ProductId in the DTO to match the one in the URL
            // This prevents updating the ProductId
            productDto.ProductId = productId;
            
            _logger.LogInformation("Updating product with ID: {ProductId}", productId);

            var productRepository = _unitOfWork.GetRepository<Product, int>();
            var product = await productRepository.GetByCondition(p => p.ProductId == productId).FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Product not found for update: {ProductId}", productId);
                return ApiResponseDto<ProductDto>.ErrorResponse($"Product with ID {productId} not found");
            }

            try
            {
                _mapper.Map(productDto, product);
                
                // Handle attributes dictionary
                if (productDto.Attributes != null && productDto.Attributes.Count > 0)
                {
                    product.SetAttributes(productDto.Attributes);
                }
                
                product.LastModifiedDate = DateTime.UtcNow;
                
                productRepository.Update(product);
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Product",
                    EntityId = product.ProductId,
                    Status = "Success",
                    Message = "Product updated successfully"
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product updated successfully: {ProductId}", productId);
                return ApiResponseDto<ProductDto>.SuccessResponse(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", productId);
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "ProductService.UpdateProduct",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "Product",
                    EntityId = productId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<ProductDto>.ErrorResponse("Error updating product", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<string>> CreateProductsBatchAsync(BatchRequestDto<ProductDto> batchRequest)
        {
            _logger.LogInformation("Processing batch product creation. Request ID: {RequestId}, Count: {Count}", 
                batchRequest.RequestId, batchRequest.Items.Count);

            var results = new List<string>();
            var errors = new List<string>();

            var productRepository = _unitOfWork.GetRepository<Product, int>();
            var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var productDto in batchRequest.Items)
                {
                    try
                    {
                        // Check if product already exists
                        var existingProduct = await productRepository.GetByCondition(p => p.ProductId == productDto.ProductId).FirstOrDefaultAsync();

                        if (existingProduct != null)
                        {
                            // Update existing product
                            _mapper.Map(productDto, existingProduct);
                            
                            // Handle attributes dictionary
                            if (productDto.Attributes != null && productDto.Attributes.Count > 0)
                            {
                                existingProduct.SetAttributes(productDto.Attributes);
                            }
                            
                            existingProduct.LastModifiedDate = DateTime.UtcNow;
                            productRepository.Update(existingProduct);
                            results.Add($"Updated product: {productDto.ProductId}");
                        }
                        else
                        {
                            // Create new product
                            var product = _mapper.Map<Product>(productDto);
                            
                            // Handle attributes dictionary
                            if (productDto.Attributes != null && productDto.Attributes.Count > 0)
                            {
                                product.SetAttributes(productDto.Attributes);
                            }
                            
                            await productRepository.CreateAsync(product);
                            results.Add($"Created product: {productDto.ProductId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing product in batch: {ProductId}", productDto.ProductId);
                        errors.Add($"Error processing product {productDto.ProductId}: {ex.Message}");
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "ProductService.CreateProductsBatch",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "Product",
                            EntityId = productDto.ProductId
                        };
                        await errorLogRepository.CreateAsync(errorLog);
                    }
                }

                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Product",
                    EntityId = batchRequest.RequestId,
                    Status = errors.Any() ? "Partial" : "Success",
                    Message = $"Batch processed: {results.Count} succeeded, {errors.Count} failed",
                    RequestData = $"Batch ID: {batchRequest.RequestId}, Items: {batchRequest.Items.Count}"
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);

                await _unitOfWork.CommitTransactionAsync();

                if (errors.Any())
                {
                    _logger.LogWarning("Batch product creation completed with errors. Request ID: {RequestId}", batchRequest.RequestId);
                    return ApiResponseDto<string>.ErrorResponse(
                        $"Batch processed with errors: {results.Count} succeeded, {errors.Count} failed",
                        errors);
                }

                _logger.LogInformation("Batch product creation completed successfully. Request ID: {RequestId}", batchRequest.RequestId);
                return ApiResponseDto<string>.SuccessResponse(
                    $"Batch ID: {batchRequest.RequestId}",
                    $"All {results.Count} products processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch: {RequestId}", batchRequest.RequestId);
                await _unitOfWork.RollbackTransactionAsync();
                
                return ApiResponseDto<string>.ErrorResponse("Error processing batch", new List<string> { ex.Message });
            }
        }
    }
}
