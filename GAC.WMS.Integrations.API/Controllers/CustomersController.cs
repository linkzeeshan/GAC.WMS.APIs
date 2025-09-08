using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WMS.Integrations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // GET: api/customers/{customerId}
        [HttpGet("{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CustomerDto>>> GetCustomer(string customerId)
        {
            var response = await _customerService.GetCustomerByIdAsync(customerId);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        // POST: api/customers
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<CustomerDto>>> CreateCustomer(CustomerDto customerDto)
        {
            var response = await _customerService.CreateCustomerAsync(customerDto);
            
            if (!response.Success)
            {
                if (response.Message.Contains("already exists"))
                {
                    return Conflict(response);
                }
                
                return BadRequest(response);
            }
            
            return CreatedAtAction(
                nameof(GetCustomer),
                new { customerId = response.Data.CustomerId },
                response
            );
        }

        // PUT: api/customers/{customerId}
        [HttpPut("{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<CustomerDto>>> UpdateCustomer(string customerId, CustomerDto customerDto)
        {
            var response = await _customerService.UpdateCustomerAsync(customerId, customerDto);
            
            if (!response.Success)
            {
                if (response.Message.Contains("not found"))
                {
                    return NotFound(response);
                }
                
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        // POST: api/customers/batch
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<string>>> CreateCustomersBatch(BatchRequestDto<CustomerDto> batchRequest)
        {
            var response = await _customerService.CreateCustomersBatchAsync(batchRequest);
            return Ok(response);
        }
    }
}
