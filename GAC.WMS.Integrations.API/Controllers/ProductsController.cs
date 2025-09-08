using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GAC.WMS.Integrations.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products/{productId}
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetProduct(string productId)
        {
            var response = await _productService.GetProductByIdAsync(productId);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        // POST: api/products
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> CreateProduct(ProductDto productDto)
        {
            var response = await _productService.CreateProductAsync(productDto);
            
            if (!response.Success)
            {
                if (response.Message.Contains("already exists"))
                {
                    return Conflict(response);
                }
                
                return BadRequest(response);
            }
            
            return CreatedAtAction(
                nameof(GetProduct),
                new { productId = response.Data.ProductId },
                response
            );
        }

        // PUT: api/products/{productId}
        [HttpPut("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateProduct(string productId, ProductDto productDto)
        {
            var response = await _productService.UpdateProductAsync(productId, productDto);
            
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

        // POST: api/products/batch
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponseDto<string>>> CreateProductsBatch(BatchRequestDto<ProductDto> batchRequest)
        {
            var response = await _productService.CreateProductsBatchAsync(batchRequest);
            return Ok(response);
        }
    }
}
