using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WMS.Integrations.API.Controllers
{
    [ApiController]
    [Route("api/sales-orders")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ILogger<SalesOrdersController> _logger;

        public SalesOrdersController(
            ISalesOrderService salesOrderService,
            ILogger<SalesOrdersController> logger)
        {
            _salesOrderService = salesOrderService;
            _logger = logger;
        }

        // GET: api/sales-orders/{soNumber}
        [HttpGet("{soNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<SalesOrderDto>>> GetSalesOrder(string soNumber)
        {
            var response = await _salesOrderService.GetSalesOrderByNumberAsync(soNumber);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        // POST: api/sales-orders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<SalesOrderDto>>> CreateSalesOrder(SalesOrderDto salesOrderDto)
        {
            var response = await _salesOrderService.CreateSalesOrderAsync(salesOrderDto);
            
            if (!response.Success)
            {
                if (response.Message.Contains("already exists"))
                {
                    return Conflict(response);
                }
                
                return BadRequest(response);
            }
            
            return CreatedAtAction(
                nameof(GetSalesOrder),
                new { soNumber = response.Data.SONumber },
                response
            );
        }

        // PUT: api/sales-orders/{soNumber}
        [HttpPut("{soNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<SalesOrderDto>>> UpdateSalesOrder(string soNumber, SalesOrderDto salesOrderDto)
        {
            var response = await _salesOrderService.UpdateSalesOrderAsync(soNumber, salesOrderDto);
            
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

        // POST: api/sales-orders/batch
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<string>>> CreateSalesOrdersBatch(BatchRequestDto<SalesOrderDto> batchRequest)
        {
            var response = await _salesOrderService.CreateSalesOrdersBatchAsync(batchRequest);
            return Ok(response);
        }
    }
}
