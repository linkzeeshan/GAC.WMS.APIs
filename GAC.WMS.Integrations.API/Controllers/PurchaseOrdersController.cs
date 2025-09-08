using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WMS.Integrations.API.Controllers
{
    [ApiController]
    [Route("api/purchase-orders")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(
            IPurchaseOrderService purchaseOrderService,
            ILogger<PurchaseOrdersController> logger)
        {
            _purchaseOrderService = purchaseOrderService;  
            _logger = logger;
        }

        // GET: api/purchase-orders/{poNumber}
        [HttpGet("{poNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<PurchaseOrderDto>>> GetPurchaseOrder(string poNumber)
        {
            var response = await _purchaseOrderService.GetPurchaseOrderByNumberAsync(poNumber);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        // POST: api/purchase-orders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<PurchaseOrderDto>>> CreatePurchaseOrder(PurchaseOrderDto purchaseOrderDto)
        {
            var response = await _purchaseOrderService.CreatePurchaseOrderAsync(purchaseOrderDto);
            
            if (!response.Success)
            {
                if (response.Message.Contains("already exists"))
                {
                    return Conflict(response);
                }
                
                return BadRequest(response);
            }
            
            return CreatedAtAction(
                nameof(GetPurchaseOrder),
                new { poNumber = response.Data.PONumber },
                response
            );
        }

        // PUT: api/purchase-orders/{poNumber}
        [HttpPut("{poNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<PurchaseOrderDto>>> UpdatePurchaseOrder(string poNumber, PurchaseOrderDto purchaseOrderDto)
        {
            var response = await _purchaseOrderService.UpdatePurchaseOrderAsync(poNumber, purchaseOrderDto);
            
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

        // POST: api/purchase-orders/batch
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<string>>> CreatePurchaseOrdersBatch(BatchRequestDto<PurchaseOrderDto> batchRequest)
        {
            var response = await _purchaseOrderService.CreatePurchaseOrdersBatchAsync(batchRequest);
            return Ok(response);
        }
    }
}
