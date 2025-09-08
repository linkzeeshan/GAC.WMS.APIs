using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GAC.WMS.Integrations.Application.Services.Implementation
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseOrderService> _logger;

        public PurchaseOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PurchaseOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponseDto<PurchaseOrderDto>> GetPurchaseOrderByNumberAsync(string poNumber)
        {
            _logger.LogInformation("Getting purchase order with number: {PONumber}", poNumber);
            
            var purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrder, int>();
            var purchaseOrder = await purchaseOrderRepository
                .GetByCondition(po => po.PONumber == poNumber)
                .Include(po => po.Customer)
                .Include(po => po.POLines)
                    .ThenInclude(pol => pol.Product)
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found: {PONumber}", poNumber);
                return ApiResponseDto<PurchaseOrderDto>.ErrorResponse($"Purchase order with number {poNumber} not found");
            }

            var purchaseOrderDto = _mapper.Map<PurchaseOrderDto>(purchaseOrder);
            return ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto);
        }

        public async Task<ApiResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(PurchaseOrderDto purchaseOrderDto)
        {
            _logger.LogInformation("Creating new purchase order with number: {PONumber}", purchaseOrderDto.PONumber);
            
            var purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrder, int>();
            
            // Check if purchase order already exists
            var existingPurchaseOrder = await purchaseOrderRepository
                .GetByCondition(po => po.PONumber == purchaseOrderDto.PONumber)
                .FirstOrDefaultAsync();
                
            if (existingPurchaseOrder != null)
            {
                _logger.LogWarning("Purchase order already exists: {PONumber}", purchaseOrderDto.PONumber);
                return ApiResponseDto<PurchaseOrderDto>.ErrorResponse($"Purchase order with number {purchaseOrderDto.PONumber} already exists");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Create a new purchase order entity (without lines)
                var purchaseOrder = new PurchaseOrder
                {
                    PONumber = purchaseOrderDto.PONumber,
                    VendorId = purchaseOrderDto.VendorId,
                    OrderDate = purchaseOrderDto.OrderDate,
                    ExpectedDeliveryDate = purchaseOrderDto.ExpectedDeliveryDate,
                    Status = purchaseOrderDto.Status,
                    Currency = purchaseOrderDto.Currency,
                    TotalAmount = purchaseOrderDto.TotalAmount,
                    ShippingStreet = purchaseOrderDto.ShippingStreet,
                    ShippingCity = purchaseOrderDto.ShippingCity,
                    ShippingStateProvince = purchaseOrderDto.ShippingStateProvince,
                    ShippingPostalCode = purchaseOrderDto.ShippingPostalCode,
                    ShippingCountry = purchaseOrderDto.ShippingCountry
                };
                
                // Find customer if customerId is provided
                if (purchaseOrderDto.CustomerId is not null && purchaseOrderDto.CustomerId is not 0)
                {
                    var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                    var customer = await customerRepository
                        .GetByCondition(c => c.Id == purchaseOrderDto.CustomerId)
                        .FirstOrDefaultAsync();
                        
                    if (customer != null)
                    {
                        purchaseOrder.CustomerId = customer.Id;
                    }
                }
                
                // Create purchase order (without lines)
                await purchaseOrderRepository.CreateAsync(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
                
                // Process line items separately
                var productRepository = _unitOfWork.GetRepository<Product, int>();
                var purchaseOrderLineRepository = _unitOfWork.GetRepository<PurchaseOrderLine, int>();
                
                foreach (var lineDto in purchaseOrderDto.POLines)
                {
                    var product = await productRepository
                        .GetByCondition(p => p.Id == lineDto.ProductId)
                        .FirstOrDefaultAsync();
                        
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {lineDto.ProductId} not found");
                    }
                    
                    // Create line item manually instead of using mapping
                    var line = new PurchaseOrderLine
                    {
                        PurchaseOrderId = purchaseOrder.Id,
                        ProductId = product.Id,
                        LineNumber = lineDto.LineNumber,
                        Quantity = lineDto.Quantity,
                        UnitPrice = lineDto.UnitPrice,
                        TotalPrice = lineDto.TotalPrice,
                        ExpectedDeliveryDate = lineDto.ExpectedDeliveryDate
                    };
                    
                    await purchaseOrderLineRepository.CreateAsync(line);
                }
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "PurchaseOrder",
                    EntityId = purchaseOrder.PONumber,
                    Status = "Success",
                    Message = "Purchase order created successfully",
                    CustomerCode = purchaseOrderDto.CustomerId.ToString()
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Purchase order created successfully: {PONumber}", purchaseOrderDto.PONumber);
                return ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto, "Purchase order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order: {PONumber}", purchaseOrderDto.PONumber);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "PurchaseOrderService.CreatePurchaseOrder",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "PurchaseOrder",
                    EntityId = purchaseOrderDto.PONumber,
                    CustomerCode = purchaseOrderDto.CustomerId.ToString()
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<PurchaseOrderDto>.ErrorResponse("Error creating purchase order", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<PurchaseOrderDto>> UpdatePurchaseOrderAsync(string poNumber, PurchaseOrderDto purchaseOrderDto)
        {
            // Force the PONumber in the DTO to match the one in the URL
            purchaseOrderDto.PONumber = poNumber;
            
            _logger.LogInformation("Updating purchase order with number: {PONumber}", poNumber);

            var purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrder, int>();
            var purchaseOrder = await purchaseOrderRepository
                .GetByCondition(po => po.PONumber == poNumber)
                .Include(po => po.POLines)
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
            {
                _logger.LogWarning("Purchase order not found for update: {PONumber}", poNumber);
                return ApiResponseDto<PurchaseOrderDto>.ErrorResponse($"Purchase order with number {poNumber} not found");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Update purchase order properties
                _mapper.Map(purchaseOrderDto, purchaseOrder);
                purchaseOrder.LastModifiedDate = DateTime.UtcNow;

                // Find customer if customerId is provided
                if (purchaseOrderDto.CustomerId is not null && purchaseOrderDto.CustomerId is not 0)
                {
                    var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                    var customer = await customerRepository
                        .GetByCondition(c => c.Id == purchaseOrderDto.CustomerId)
                        .FirstOrDefaultAsync();
                        
                    if (customer != null)
                    {
                        purchaseOrder.CustomerId = customer.Id;
                    }
                }
                
                purchaseOrderRepository.Update(purchaseOrder);
                
                // Process line items
                var productRepository = _unitOfWork.GetRepository<Product, int>();
                var purchaseOrderLineRepository = _unitOfWork.GetRepository<PurchaseOrderLine, int>();
                
                // Remove existing lines by first loading them from the database
                var existingLines = await purchaseOrderLineRepository
                    .GetByCondition(l => l.PurchaseOrderId == purchaseOrder.Id)
                    .ToListAsync();

                if (existingLines.Count() > 0)
                    purchaseOrderLineRepository.DeleteRange(existingLines);

                //foreach (var line in existingLines)
                //{
                //    purchaseOrderLineRepository.Delete(line);
                //}
                
                // Add new lines
                foreach (var lineDto in purchaseOrderDto.POLines)
                {
                    var product = await productRepository
                        .GetByCondition(p => p.Id == lineDto.ProductId)
                        .FirstOrDefaultAsync();
                        
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {lineDto.ProductId} not found");
                    }
                    
                    var line = _mapper.Map<PurchaseOrderLine>(lineDto);
                    line.PurchaseOrderId = purchaseOrder.Id;
                    line.ProductId = product.Id;
                    
                    await purchaseOrderLineRepository.CreateAsync(line);
                }
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "PurchaseOrder",
                    EntityId = purchaseOrder.PONumber,
                    Status = "Success",
                    Message = "Purchase order updated successfully",
                    CustomerCode = purchaseOrderDto.CustomerId.ToString()
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Purchase order updated successfully: {PONumber}", poNumber);
                return ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto, "Purchase order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order: {PONumber}", poNumber);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "PurchaseOrderService.UpdatePurchaseOrder",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "PurchaseOrder",
                    EntityId = poNumber,
                    CustomerCode = purchaseOrderDto.CustomerId.ToString()
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<PurchaseOrderDto>.ErrorResponse("Error updating purchase order", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<string>> CreatePurchaseOrdersBatchAsync(BatchRequestDto<PurchaseOrderDto> batchRequest)
        {
            _logger.LogInformation("Processing batch purchase order creation. Request ID: {RequestId}, Count: {Count}", 
                batchRequest.RequestId, batchRequest.Items.Count);

            var results = new List<string>();
            var errors = new List<string>();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var purchaseOrderDto in batchRequest.Items)
                {
                    try
                    {
                        var purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrder, int>();
                        
                        // Check if purchase order already exists
                        var existingPurchaseOrder = await purchaseOrderRepository
                            .GetByCondition(po => po.PONumber == purchaseOrderDto.PONumber)
                            .Include(po => po.POLines)
                            .FirstOrDefaultAsync();
                            
                        if (existingPurchaseOrder != null)
                        {
                            // Update existing purchase order
                            _mapper.Map(purchaseOrderDto, existingPurchaseOrder);
                            existingPurchaseOrder.LastModifiedDate = DateTime.UtcNow;

                            // Find customer if customerId is provided
                            if (purchaseOrderDto.CustomerId is not null && purchaseOrderDto.CustomerId is not 0)
                            {
                                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                                var customer = await customerRepository
                                    .GetByCondition(c => c.Id == purchaseOrderDto.CustomerId)
                                    .FirstOrDefaultAsync();
                                    
                                if (customer != null)
                                {
                                    existingPurchaseOrder.CustomerId = customer.Id;
                                }
                            }
                            
                            purchaseOrderRepository.Update(existingPurchaseOrder);
                            
                            // Process line items
                            var productRepository = _unitOfWork.GetRepository<Product, int>();
                            var purchaseOrderLineRepository = _unitOfWork.GetRepository<PurchaseOrderLine, int>();
                            
                            // Remove existing lines by first loading them from the database
                            var existingLines = await purchaseOrderLineRepository
                                .GetByCondition(l => l.PurchaseOrderId == existingPurchaseOrder.Id)
                                .ToListAsync();
                                
                            foreach (var line in existingLines)
                            {
                                purchaseOrderLineRepository.Delete(line);
                            }
                            
                            // Add new lines
                            foreach (var lineDto in purchaseOrderDto.POLines)
                            {
                                var product = await productRepository
                                    .GetByCondition(p => p.Id == lineDto.ProductId)
                                    .FirstOrDefaultAsync();
                                    
                                if (product == null)
                                {
                                    throw new Exception($"Product with ID {lineDto.ProductId} not found");
                                }
                                
                                var line = _mapper.Map<PurchaseOrderLine>(lineDto);
                                line.PurchaseOrderId = existingPurchaseOrder.Id;
                                line.ProductId = product.Id;
                                
                                await purchaseOrderLineRepository.CreateAsync(line);
                            }
                            
                            results.Add($"Updated purchase order: {purchaseOrderDto.PONumber}");
                        }
                        else
                        {
                            // Create new purchase order
                            var purchaseOrder = _mapper.Map<PurchaseOrder>(purchaseOrderDto);

                            // Find customer if customerId is provided
                            if (purchaseOrderDto.CustomerId is not null && purchaseOrderDto.CustomerId is not 0)
                            {
                                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                                var customer = await customerRepository
                                    .GetByCondition(c => c.Id == purchaseOrderDto.CustomerId)
                                    .FirstOrDefaultAsync();
                                    
                                if (customer != null)
                                {
                                    purchaseOrder.CustomerId = customer.Id;
                                }
                            }
                            
                            // Create purchase order
                            await purchaseOrderRepository.CreateAsync(purchaseOrder);
                            await _unitOfWork.SaveChangesAsync();
                            
                            // Process line items
                            var productRepository = _unitOfWork.GetRepository<Product, int>();
                            var purchaseOrderLineRepository = _unitOfWork.GetRepository<PurchaseOrderLine, int>();
                            
                            foreach (var lineDto in purchaseOrderDto.POLines)
                            {
                                var product = await productRepository
                                    .GetByCondition(p => p.Id == lineDto.ProductId)
                                    .FirstOrDefaultAsync();
                                    
                                if (product == null)
                                {
                                    throw new Exception($"Product with ID {lineDto.ProductId} not found");
                                }
                                
                                var line = _mapper.Map<PurchaseOrderLine>(lineDto);
                                line.PurchaseOrderId = purchaseOrder.Id;
                                line.ProductId = product.Id;
                                
                                await purchaseOrderLineRepository.CreateAsync(line);
                            }
                            
                            results.Add($"Created purchase order: {purchaseOrderDto.PONumber}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing purchase order in batch: {PONumber}", purchaseOrderDto.PONumber);
                        errors.Add($"Error processing purchase order {purchaseOrderDto.PONumber}: {ex.Message}");
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "PurchaseOrderService.CreatePurchaseOrdersBatch",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "PurchaseOrder",
                            EntityId = purchaseOrderDto.PONumber,
                            CustomerCode = purchaseOrderDto.CustomerId.ToString()
                        };
                        
                        var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                        await errorLogRepository.CreateAsync(errorLog);
                    }
                }

                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "PurchaseOrder",
                    EntityId = batchRequest.RequestId,
                    Status = errors.Any() ? "Partial" : "Success",
                    Message = $"Batch processed: {results.Count} succeeded, {errors.Count} failed",
                    RequestData = $"Batch ID: {batchRequest.RequestId}, Items: {batchRequest.Items.Count}"
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (errors.Any())
                {
                    _logger.LogWarning("Batch purchase order creation completed with errors. Request ID: {RequestId}", batchRequest.RequestId);
                    return ApiResponseDto<string>.ErrorResponse(
                        $"Batch processed with errors: {results.Count} succeeded, {errors.Count} failed",
                        errors);
                }

                _logger.LogInformation("Batch purchase order creation completed successfully. Request ID: {RequestId}", batchRequest.RequestId);
                return ApiResponseDto<string>.SuccessResponse(
                    $"Batch ID: {batchRequest.RequestId}",
                    $"All {results.Count} purchase orders processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch: {RequestId}", batchRequest.RequestId);
                await _unitOfWork.RollbackTransactionAsync();
                
                return ApiResponseDto<string>.ErrorResponse("Error processing batch", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<List<PurchaseOrderDto>>> GetPurchaseOrdersByCustomerIdAsync(string customerId)
        {
            _logger.LogInformation("Getting purchase orders for customer: {CustomerId}", customerId);
            
            try
            {
                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                var customer = await customerRepository
                    .GetByCondition(c => c.CustomerId == customerId)
                    .FirstOrDefaultAsync();
                    
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                    return ApiResponseDto<List<PurchaseOrderDto>>.ErrorResponse($"Customer with ID {customerId} not found");
                }
                
                var purchaseOrderRepository = _unitOfWork.GetRepository<PurchaseOrder, int>();
                var purchaseOrders = await purchaseOrderRepository
                    .GetByCondition(po => po.CustomerId == customer.Id)
                    .Include(po => po.POLines)
                        .ThenInclude(pol => pol.Product)
                    .ToListAsync();
                
                var purchaseOrderDtos = _mapper.Map<List<PurchaseOrderDto>>(purchaseOrders);
                return ApiResponseDto<List<PurchaseOrderDto>>.SuccessResponse(purchaseOrderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase orders for customer: {CustomerId}", customerId);
                return ApiResponseDto<List<PurchaseOrderDto>>.ErrorResponse("Error getting purchase orders", new List<string> { ex.Message });
            }
        }
    }
}
