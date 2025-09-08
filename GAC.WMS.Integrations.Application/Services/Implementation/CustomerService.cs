using AutoMapper;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GAC.WMS.Integrations.Application.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponseDto<CustomerDto>> GetCustomerByIdAsync(string customerId)
        {
            _logger.LogInformation("Getting customer with ID: {CustomerId}", customerId);
            
            var customerRepository = _unitOfWork.GetRepository<Customer, int>();
            var customer = await customerRepository.GetByCondition(c => c.CustomerId == customerId).FirstOrDefaultAsync();

            if (customer == null)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", customerId);
                return ApiResponseDto<CustomerDto>.ErrorResponse($"Customer with ID {customerId} not found");
            }

            var customerDto = _mapper.Map<CustomerDto>(customer);
            return ApiResponseDto<CustomerDto>.SuccessResponse(customerDto);
        }

        public async Task<ApiResponseDto<CustomerDto>> CreateCustomerAsync(CustomerDto customerDto)
        {
            // Ensure CustomerId is set (will use the default if not provided)
            if (string.IsNullOrEmpty(customerDto.CustomerId))
            {
                customerDto.CustomerId = Guid.CreateVersion7().ToString();
            }
            
            _logger.LogInformation("Creating new customer with ID: {CustomerId}", customerDto.CustomerId);
            
            var customerRepository = _unitOfWork.GetRepository<Customer, int>();
            
            // Check if customer already exists
            var existingCustomer = await customerRepository.GetByCondition(c => c.CustomerId == customerDto.CustomerId).FirstOrDefaultAsync();
            if (existingCustomer != null)
            {
                _logger.LogWarning("Customer already exists: {CustomerId}", customerDto.CustomerId);
                return ApiResponseDto<CustomerDto>.ErrorResponse($"Customer with ID {customerDto.CustomerId} already exists");
            }

            try
            {
                var customer = _mapper.Map<Customer>(customerDto);
                await customerRepository.CreateAsync(customer);
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Customer",
                    EntityId = customer.CustomerId,
                    Status = "Success",
                    Message = "Customer created successfully",
                    CustomerCode = customer.CustomerId
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer created successfully: {CustomerId}", customerDto.CustomerId);
                return ApiResponseDto<CustomerDto>.SuccessResponse(customerDto, "Customer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerId}", customerDto.CustomerId);
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "CustomerService.CreateCustomer",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "Customer",
                    EntityId = customerDto.CustomerId,
                    CustomerCode = customerDto.CustomerId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<CustomerDto>.ErrorResponse("Error creating customer", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<CustomerDto>> UpdateCustomerAsync(string customerId, CustomerDto customerDto)
        {
            // Force the CustomerId in the DTO to match the one in the URL
            // This prevents updating the CustomerId
            customerDto.CustomerId = customerId;
            
            _logger.LogInformation("Updating customer with ID: {CustomerId}", customerId);

            var customerRepository = _unitOfWork.GetRepository<Customer, int>();
            var customer = await customerRepository.GetByCondition(c => c.CustomerId == customerId).FirstOrDefaultAsync();

            if (customer == null)
            {
                _logger.LogWarning("Customer not found for update: {CustomerId}", customerId);
                return ApiResponseDto<CustomerDto>.ErrorResponse($"Customer with ID {customerId} not found");
            }

            try
            {
                _mapper.Map(customerDto, customer);
                customer.LastModifiedDate = DateTime.UtcNow;
                
                customerRepository.Update(customer);
                
                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Customer",
                    EntityId = customer.CustomerId,
                    Status = "Success",
                    Message = "Customer updated successfully",
                    CustomerCode = customer.CustomerId
                };
                
                var integrationLogRepository = _unitOfWork.GetRepository<IntegrationLog, int>();
                await integrationLogRepository.CreateAsync(integrationLog);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer updated successfully: {CustomerId}", customerId);
                return ApiResponseDto<CustomerDto>.SuccessResponse(customerDto, "Customer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "CustomerService.UpdateCustomer",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "Customer",
                    EntityId = customerId,
                    CustomerCode = customerId
                };
                
                var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();
                await errorLogRepository.CreateAsync(errorLog);
                await _unitOfWork.SaveChangesAsync();
                
                return ApiResponseDto<CustomerDto>.ErrorResponse("Error updating customer", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponseDto<string>> CreateCustomersBatchAsync(BatchRequestDto<CustomerDto> batchRequest)
        {
            _logger.LogInformation("Processing batch customer creation. Request ID: {RequestId}, Count: {Count}", 
                batchRequest.RequestId, batchRequest.Items.Count);

            var results = new List<string>();
            var errors = new List<string>();

            var customerRepository = _unitOfWork.GetRepository<Customer, int>();
            var errorLogRepository = _unitOfWork.GetRepository<ErrorLog, int>();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var customerDto in batchRequest.Items)
                {
                    try
                    {
                        // Check if customer already exists
                        var existingCustomer = await customerRepository.GetByCondition(c => c.CustomerId == customerDto.CustomerId).FirstOrDefaultAsync();

                        if (existingCustomer != null)
                        {
                            // Update existing customer
                            _mapper.Map(customerDto, existingCustomer);
                            existingCustomer.LastModifiedDate = DateTime.UtcNow;
                            customerRepository.Update(existingCustomer);
                            results.Add($"Updated customer: {customerDto.CustomerId}");
                        }
                        else
                        {
                            // Create new customer
                            var customer = _mapper.Map<Customer>(customerDto);
                            await customerRepository.CreateAsync(customer);
                            results.Add($"Created customer: {customerDto.CustomerId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing customer in batch: {CustomerId}", customerDto.CustomerId);
                        errors.Add($"Error processing customer {customerDto.CustomerId}: {ex.Message}");
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "CustomerService.CreateCustomersBatch",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "Customer",
                            EntityId = customerDto.CustomerId,
                            CustomerCode = customerDto.CustomerId
                        };
                        await errorLogRepository.CreateAsync(errorLog);
                    }
                }

                // Log integration activity
                var integrationLog = new IntegrationLog
                {
                    IntegrationType = "API",
                    EntityType = "Customer",
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
                    _logger.LogWarning("Batch customer creation completed with errors. Request ID: {RequestId}", batchRequest.RequestId);
                    return ApiResponseDto<string>.ErrorResponse(
                        $"Batch processed with errors: {results.Count} succeeded, {errors.Count} failed",
                        errors);
                }

                _logger.LogInformation("Batch customer creation completed successfully. Request ID: {RequestId}", batchRequest.RequestId);
                return ApiResponseDto<string>.SuccessResponse(
                    $"Batch ID: {batchRequest.RequestId}",
                    $"All {results.Count} customers processed successfully");
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
