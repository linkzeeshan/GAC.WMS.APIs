using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;

namespace GAC.WMS.Integrations.Application.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponseDto<CustomerDto>> GetCustomerByIdAsync(string customerId);
        Task<ApiResponseDto<CustomerDto>> CreateCustomerAsync(CustomerDto customerDto);
        Task<ApiResponseDto<CustomerDto>> UpdateCustomerAsync(string customerId, CustomerDto customerDto);
        Task<ApiResponseDto<string>> CreateCustomersBatchAsync(BatchRequestDto<CustomerDto> batchRequest);
    }
}
