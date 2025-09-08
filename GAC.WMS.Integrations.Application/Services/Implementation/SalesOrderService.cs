using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GAC.WMS.Integrations.Application.Services.Implementation
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SalesOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponseDto<SalesOrderDto>> GetSalesOrderByNumberAsync(string soNumber)
        {
            _logger.LogInformation("Getting sales order with number: {SONumber}", soNumber);
            
            var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
            var salesOrder = await salesOrderRepository
                .GetByCondition(so => so.SONumber == soNumber)
                .Include(so => so.CustomerEntity)
                .Include(so => so.SOLines)
                    .ThenInclude(sol => sol.Product)
                .FirstOrDefaultAsync();

            if (salesOrder == null)
            {
                _logger.LogWarning("Sales order not found: {SONumber}", soNumber);
                return ApiResponseDto<SalesOrderDto>.ErrorResponse($"Sales order with number {soNumber} not found");
            }

            var salesOrderDto = _mapper.Map<SalesOrderDto>(salesOrder);
            return ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto);
        }

        public async Task<ApiResponseDto<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderDto salesOrderDto)
        {
            _logger.LogInformation("Creating new sales order with number: {SONumber}", salesOrderDto.SONumber);
            
            var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
            
            // Check if sales order already exists
            var existingSalesOrder = await salesOrderRepository
                .GetByCondition(so => so.SONumber == salesOrderDto.SONumber)
                .FirstOrDefaultAsync();
                
            if (existingSalesOrder != null)
            {
                _logger.LogWarning("Sales order already exists: {SONumber}", salesOrderDto.SONumber);
                return ApiResponseDto<SalesOrderDto>.ErrorResponse($"Sales order with number {salesOrderDto.SONumber} already exists");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Create a new sales order entity (without lines)
                var salesOrder = new SalesOrder
                {
                    SONumber = salesOrderDto.SONumber,
                    OrderDate = salesOrderDto.OrderDate,
                    RequestedDeliveryDate = salesOrderDto.RequestedDeliveryDate,
                    Status = salesOrderDto.Status,
                    Currency = salesOrderDto.Currency,
                    TotalAmount = salesOrderDto.TotalAmount,
                    ShippingStreet = salesOrderDto.ShippingStreet,
                    ShippingCity = salesOrderDto.ShippingCity,
                    ShippingStateProvince = salesOrderDto.ShippingStateProvince,
                    ShippingPostalCode = salesOrderDto.ShippingPostalCode,
                    ShippingCountry = salesOrderDto.ShippingCountry,
                    CustomerId = salesOrderDto.CustomerId
                };
                
                // Find customer if customerId is provided
                if (!string.IsNullOrEmpty(salesOrderDto.CustomerId))
                {
                    var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                    var customer = await customerRepository
                        .GetByCondition(c => c.CustomerId == salesOrderDto.CustomerId)
                        .FirstOrDefaultAsync();
                        
                    if (customer != null)
                    {
                        salesOrder.CustomerEntityId = customer.Id;
                    }
                }
                
                // Create sales order (without lines)
                await salesOrderRepository.CreateAsync(salesOrder);
                await _unitOfWork.SaveChangesAsync();
                
                // Process line items separately
                var productRepository = _unitOfWork.GetRepository<Product, int>();
                var salesOrderLineRepository = _unitOfWork.GetRepository<SalesOrderLine, int>();
                
                foreach (var lineDto in salesOrderDto.SOLines)
                {
                    var product = await productRepository
                        .GetByCondition(p => p.Id == lineDto.ProductId)
                        .FirstOrDefaultAsync();
                        
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {lineDto.ProductId} not found");
                    }
                    
                    // Create line item manually instead of using mapping
                    var line = new SalesOrderLine
                    {
                        SalesOrderId = salesOrder.Id,
                        ProductId = product.Id,
                        LineNumber = lineDto.LineNumber,
                        Quantity = lineDto.Quantity,
                        UnitPrice = lineDto.UnitPrice,
                        TotalPrice = lineDto.TotalPrice,
                        RequestedDeliveryDate = lineDto.RequestedDeliveryDate
                    };
                    
                    await salesOrderLineRepository.CreateAsync(line);
                }
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "SalesOrder",
                    EntityId = salesOrder.SONumber,
                    Status = "Success",
                    Message = "Sales order created successfully",
                    CustomerCode = salesOrderDto.CustomerId
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Sales order created successfully: {SONumber}", salesOrderDto.SONumber);
                return ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto, "Sales order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sales order: {SONumber}", salesOrderDto.SONumber);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "SalesOrderService.CreateSalesOrder",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "SalesOrder",
                    EntityId = salesOrderDto.SONumber,
                    CustomerCode = salesOrderDto.CustomerId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<SalesOrderDto>.ErrorResponse("Error creating sales order", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<SalesOrderDto>> UpdateSalesOrderAsync(string soNumber, SalesOrderDto salesOrderDto)
        {
            // Force the SONumber in the DTO to match the one in the URL
            salesOrderDto.SONumber = soNumber;
            
            _logger.LogInformation("Updating sales order with number: {SONumber}", soNumber);

            var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
            var salesOrder = await salesOrderRepository
                .GetByCondition(so => so.SONumber == soNumber)
                .Include(so => so.SOLines)
                .FirstOrDefaultAsync();
                
            if (salesOrder == null)
            {
                _logger.LogWarning("Sales order not found: {SONumber}", soNumber);
                return ApiResponseDto<SalesOrderDto>.ErrorResponse($"Sales order with number {soNumber} not found");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                // Update sales order properties
                _mapper.Map(salesOrderDto, salesOrder);
                salesOrder.LastModifiedDate = DateTime.UtcNow;
                
                // Find customer if customerId is provided
                if (!string.IsNullOrEmpty(salesOrderDto.CustomerId))
                {
                    var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                    var customer = await customerRepository
                        .GetByCondition(c => c.CustomerId == salesOrderDto.CustomerId)
                        .FirstOrDefaultAsync();
                        
                    if (customer != null)
                    {
                        salesOrder.CustomerEntityId = customer.Id;
                    }
                }
                
                salesOrderRepository.Update(salesOrder);
                
                // Process line items
                var productRepository = _unitOfWork.GetRepository<Product, int>();
                var salesOrderLineRepository = _unitOfWork.GetRepository<SalesOrderLine, int>();
                
                // Remove existing lines by first loading them from the database
                var existingLines = await salesOrderLineRepository
                    .GetByCondition(l => l.SalesOrderId == salesOrder.Id)
                    .ToListAsync();
                    
                if (existingLines.Count() > 0)
                    salesOrderLineRepository.DeleteRange(existingLines);
                
                // Add new lines
                foreach (var lineDto in salesOrderDto.SOLines)
                {
                    var product = await productRepository
                        .GetByCondition(p => p.Id == lineDto.ProductId)
                        .FirstOrDefaultAsync();
                        
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {lineDto.ProductId} not found");
                    }
                    
                    var line = new SalesOrderLine
                    {
                        SalesOrderId = salesOrder.Id,
                        ProductId = product.Id,
                        LineNumber = lineDto.LineNumber,
                        Quantity = lineDto.Quantity,
                        UnitPrice = lineDto.UnitPrice,
                        TotalPrice = lineDto.TotalPrice,
                        RequestedDeliveryDate = lineDto.RequestedDeliveryDate
                    };
                    
                    await salesOrderLineRepository.CreateAsync(line);
                }
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "SalesOrder",
                    EntityId = salesOrder.SONumber,
                    Status = "Success",
                    Message = "Sales order updated successfully",
                    CustomerCode = salesOrderDto.CustomerId
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Sales order updated successfully: {SONumber}", soNumber);
                return ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto, "Sales order updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sales order: {SONumber}", soNumber);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "SalesOrderService.UpdateSalesOrder",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "SalesOrder",
                    EntityId = soNumber,
                    CustomerCode = salesOrderDto.CustomerId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<SalesOrderDto>.ErrorResponse("Error updating sales order", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<List<SalesOrderDto>>> ProcessBatchSalesOrdersAsync(BatchRequestDto<SalesOrderDto> batchRequest)
        {
            _logger.LogInformation("Processing batch of {Count} sales orders", batchRequest.Items.Count);
            
            var results = new List<SalesOrderDto>();
            var errors = new List<string>();
            
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                foreach (var salesOrderDto in batchRequest.Items)
                {
                    try
                    {
                        var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
                        
                        // Check if sales order already exists
                        var existingSalesOrder = await salesOrderRepository
                            .GetByCondition(so => so.SONumber == salesOrderDto.SONumber)
                            .Include(so => so.SOLines)
                            .FirstOrDefaultAsync();
                            
                        if (existingSalesOrder != null)
                        {
                            // Update existing sales order
                            _mapper.Map(salesOrderDto, existingSalesOrder);
                            existingSalesOrder.LastModifiedDate = DateTime.UtcNow;

                            // Find customer if customerId is provided
                            if (!string.IsNullOrEmpty(salesOrderDto.CustomerId))
                            {
                                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                                var customer = await customerRepository
                                    .GetByCondition(c => c.CustomerId == salesOrderDto.CustomerId)
                                    .FirstOrDefaultAsync();
                                    
                                if (customer != null)
                                {
                                    existingSalesOrder.CustomerEntityId = customer.Id;
                                }
                            }
                            
                            salesOrderRepository.Update(existingSalesOrder);
                            
                            // Process line items
                            var productRepository = _unitOfWork.GetRepository<Product, int>();
                            var salesOrderLineRepository = _unitOfWork.GetRepository<SalesOrderLine, int>();
                            
                            // Remove existing lines by first loading them from the database
                            var existingLines = await salesOrderLineRepository
                                .GetByCondition(l => l.SalesOrderId == existingSalesOrder.Id)
                                .ToListAsync();
                                
                            if (existingLines.Count() > 0)
                                salesOrderLineRepository.DeleteRange(existingLines);
                            
                            // Add new lines
                            foreach (var lineDto in salesOrderDto.SOLines)
                            {
                                var product = await productRepository
                                    .GetByCondition(p => p.Id == lineDto.ProductId)
                                    .FirstOrDefaultAsync();
                                    
                                if (product == null)
                                {
                                    throw new Exception($"Product with ID {lineDto.ProductId} not found");
                                }
                                
                                var line = new SalesOrderLine
                                {
                                    SalesOrderId = existingSalesOrder.Id,
                                    ProductId = product.Id,
                                    LineNumber = lineDto.LineNumber,
                                    Quantity = lineDto.Quantity,
                                    UnitPrice = lineDto.UnitPrice,
                                    TotalPrice = lineDto.TotalPrice,
                                    RequestedDeliveryDate = lineDto.RequestedDeliveryDate
                                };
                                
                                await salesOrderLineRepository.CreateAsync(line);
                            }
                            
                            results.Add(salesOrderDto);
                        }
                        else
                        {
                            // Create new sales order
                            var salesOrder = new SalesOrder
                            {
                                SONumber = salesOrderDto.SONumber,
                                OrderDate = salesOrderDto.OrderDate,
                                RequestedDeliveryDate = salesOrderDto.RequestedDeliveryDate,
                                Status = salesOrderDto.Status,
                                Currency = salesOrderDto.Currency,
                                TotalAmount = salesOrderDto.TotalAmount,
                                ShippingStreet = salesOrderDto.ShippingStreet,
                                ShippingCity = salesOrderDto.ShippingCity,
                                ShippingStateProvince = salesOrderDto.ShippingStateProvince,
                                ShippingPostalCode = salesOrderDto.ShippingPostalCode,
                                ShippingCountry = salesOrderDto.ShippingCountry,
                                CustomerId = salesOrderDto.CustomerId
                            };

                            // Find customer if customerId is provided
                            if (!string.IsNullOrEmpty(salesOrderDto.CustomerId))
                            {
                                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                                var customer = await customerRepository
                                    .GetByCondition(c => c.CustomerId == salesOrderDto.CustomerId)
                                    .FirstOrDefaultAsync();
                                    
                                if (customer != null)
                                {
                                    salesOrder.CustomerEntityId = customer.Id;
                                }
                            }
                            
                            await salesOrderRepository.CreateAsync(salesOrder);
                            await _unitOfWork.SaveChangesAsync();
                            
                            // Process line items
                            var productRepository = _unitOfWork.GetRepository<Product, int>();
                            var salesOrderLineRepository = _unitOfWork.GetRepository<SalesOrderLine, int>();
                            
                            foreach (var lineDto in salesOrderDto.SOLines)
                            {
                                var product = await productRepository
                                    .GetByCondition(p => p.Id == lineDto.ProductId)
                                    .FirstOrDefaultAsync();
                                    
                                if (product == null)
                                {
                                    throw new Exception($"Product with ID {lineDto.ProductId} not found");
                                }
                                
                                var line = new SalesOrderLine
                                {
                                    SalesOrderId = salesOrder.Id,
                                    ProductId = product.Id,
                                    LineNumber = lineDto.LineNumber,
                                    Quantity = lineDto.Quantity,
                                    UnitPrice = lineDto.UnitPrice,
                                    TotalPrice = lineDto.TotalPrice,
                                    RequestedDeliveryDate = lineDto.RequestedDeliveryDate
                                };
                                
                                await salesOrderLineRepository.CreateAsync(line);
                            }
                            
                            results.Add(salesOrderDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing sales order: {SONumber}", salesOrderDto.SONumber);
                        errors.Add($"Error processing sales order {salesOrderDto.SONumber}: {ex.Message}");
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "SalesOrderService.ProcessBatchSalesOrders",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "SalesOrder",
                            EntityId = salesOrderDto.SONumber,
                            CustomerCode = salesOrderDto.CustomerId
                        };
                        
                        var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                        await errorLogRepository.CreateAsync(errorLog);
                    }
                }
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (errors.Count > 0)
                {
                    _logger.LogWarning("Batch processing completed with {ErrorCount} errors", errors.Count);
                    return ApiResponseDto<List<SalesOrderDto>>.ErrorResponse("Some sales orders failed to process", errors);
                }
                
                _logger.LogInformation("Batch processing completed successfully for {Count} sales orders", results.Count);
                return ApiResponseDto<List<SalesOrderDto>>.SuccessResponse(results, "All sales orders processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of sales orders");
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return ApiResponseDto<List<SalesOrderDto>>.ErrorResponse("Error processing batch of sales orders", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<string>> CreateSalesOrdersBatchAsync(BatchRequestDto<SalesOrderDto> batchRequest)
        {
            _logger.LogInformation("Processing batch sales order creation. Request ID: {RequestId}, Count: {Count}", 
                batchRequest.RequestId, batchRequest.Items.Count);

            var results = new List<string>();
            var errors = new List<string>();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var salesOrderDto in batchRequest.Items)
                {
                    try
                    {
                        var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
                        
                        // Check if sales order already exists
                        var existingSalesOrder = await salesOrderRepository
                            .GetByCondition(so => so.SONumber == salesOrderDto.SONumber)
                            .FirstOrDefaultAsync();
                            
                        if (existingSalesOrder != null)
                        {
                            errors.Add($"Sales order with number {salesOrderDto.SONumber} already exists");
                            continue;
                        }
                        
                        // Create a new sales order entity (without lines)
                        var salesOrder = new SalesOrder
                        {
                            SONumber = salesOrderDto.SONumber,
                            OrderDate = salesOrderDto.OrderDate,
                            RequestedDeliveryDate = salesOrderDto.RequestedDeliveryDate,
                            Status = salesOrderDto.Status,
                            Currency = salesOrderDto.Currency,
                            TotalAmount = salesOrderDto.TotalAmount,
                            ShippingStreet = salesOrderDto.ShippingStreet,
                            ShippingCity = salesOrderDto.ShippingCity,
                            ShippingStateProvince = salesOrderDto.ShippingStateProvince,
                            ShippingPostalCode = salesOrderDto.ShippingPostalCode,
                            ShippingCountry = salesOrderDto.ShippingCountry,
                            CustomerId = salesOrderDto.CustomerId
                        };

                        // Find customer if customerId is provided
                        if (!string.IsNullOrEmpty(salesOrderDto.CustomerId))
                        {
                            var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                            var customer = await customerRepository
                                .GetByCondition(c => c.CustomerId == salesOrderDto.CustomerId)
                                .FirstOrDefaultAsync();
                                
                            if (customer != null)
                            {
                                salesOrder.CustomerEntityId = customer.Id;
                            }
                        }
                        
                        await salesOrderRepository.CreateAsync(salesOrder);
                        await _unitOfWork.SaveChangesAsync();
                        
                        // Process line items
                        var productRepository = _unitOfWork.GetRepository<Product, int>();
                        var salesOrderLineRepository = _unitOfWork.GetRepository<SalesOrderLine, int>();
                        
                        foreach (var lineDto in salesOrderDto.SOLines)
                        {
                            var product = await productRepository
                                .GetByCondition(p => p.Id == lineDto.ProductId)
                                .FirstOrDefaultAsync();
                                
                            if (product == null)
                            {
                                throw new Exception($"Product with ID {lineDto.ProductId} not found");
                            }
                            
                            var line = new SalesOrderLine
                            {
                                SalesOrderId = salesOrder.Id,
                                ProductId = product.Id,
                                LineNumber = lineDto.LineNumber,
                                Quantity = lineDto.Quantity,
                                UnitPrice = lineDto.UnitPrice,
                                TotalPrice = lineDto.TotalPrice,
                                RequestedDeliveryDate = lineDto.RequestedDeliveryDate
                            };
                            
                            await salesOrderLineRepository.CreateAsync(line);
                        }
                        
                        results.Add($"Created sales order: {salesOrderDto.SONumber}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing sales order in batch: {SONumber}", salesOrderDto.SONumber);
                        errors.Add($"Error processing sales order {salesOrderDto.SONumber}: {ex.Message}");
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "SalesOrderService.CreateSalesOrdersBatch",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "SalesOrder",
                            EntityId = salesOrderDto.SONumber,
                            CustomerCode = salesOrderDto.CustomerId
                        };
                        
                        var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                        await errorLogRepository.CreateAsync(errorLog);
                    }
                }

                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "SalesOrder",
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
                    _logger.LogWarning("Batch sales order creation completed with errors. Request ID: {RequestId}", batchRequest.RequestId);
                    return ApiResponseDto<string>.ErrorResponse(
                        $"Batch processed with errors: {results.Count} succeeded, {errors.Count} failed",
                        errors);
                }

                _logger.LogInformation("Batch sales order creation completed successfully. Request ID: {RequestId}", batchRequest.RequestId);
                return ApiResponseDto<string>.SuccessResponse(
                    $"Batch ID: {batchRequest.RequestId}",
                    $"All {results.Count} sales orders processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch: {RequestId}", batchRequest.RequestId);
                await _unitOfWork.RollbackTransactionAsync();
                
                return ApiResponseDto<string>.ErrorResponse("Error processing batch", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<List<SalesOrderDto>>> GetSalesOrdersByCustomerIdAsync(string customerId)
        {
            _logger.LogInformation("Getting sales orders for customer: {CustomerId}", customerId);
            
            try
            {
                var customerRepository = _unitOfWork.GetRepository<Customer, int>();
                var customer = await customerRepository
                    .GetByCondition(c => c.CustomerId == customerId)
                    .FirstOrDefaultAsync();
                    
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                    return ApiResponseDto<List<SalesOrderDto>>.ErrorResponse($"Customer with ID {customerId} not found");
                }
                
                var salesOrderRepository = _unitOfWork.GetRepository<SalesOrder, int>();
                
                // Check both CustomerEntityId and CustomerId fields
                var salesOrders = await salesOrderRepository
                    .GetByCondition(so => so.CustomerEntityId == customer.Id || so.CustomerId == customerId)
                    .Include(so => so.SOLines)
                        .ThenInclude(sol => sol.Product)
                    .ToListAsync();
                
                var salesOrderDtos = _mapper.Map<List<SalesOrderDto>>(salesOrders);
                return ApiResponseDto<List<SalesOrderDto>>.SuccessResponse(salesOrderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales orders for customer: {CustomerId}", customerId);
                return ApiResponseDto<List<SalesOrderDto>>.ErrorResponse("Error getting sales orders", new List<string> { ex.Message });
            }
        }
    }
}
