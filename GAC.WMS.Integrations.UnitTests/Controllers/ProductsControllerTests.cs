using FluentAssertions;
using GAC.WMS.Integrations.API.Controllers;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GAC.WMS.Integrations.UnitTests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var productId = "PROD123";
            var productDto = new ProductDto { ProductId = productId, Name = "Test Product" };
            var apiResponse = ApiResponseDto<ProductDto>.SuccessResponse(productDto);
            
            _mockProductService
                .Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(okResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = "INVALID_ID";
            var apiResponse = ApiResponseDto<ProductDto>.ErrorResponse($"Product with ID {productId} not found");
            
            _mockProductService
                .Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(notFoundResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain(productId);
        }

        [Fact]
        public async Task CreateProduct_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var productDto = new ProductDto { ProductId = "PROD123", Name = "Test Product" };
            var apiResponse = ApiResponseDto<ProductDto>.SuccessResponse(productDto);
            
            _mockProductService
                .Setup(service => service.CreateProductAsync(It.IsAny<ProductDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(createdAtActionResult.Value);
            
            createdAtActionResult.ActionName.Should().Be(nameof(ProductsController.GetProduct));
            createdAtActionResult.RouteValues["productId"].Should().Be(productDto.ProductId);
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_WithDuplicateId_ReturnsConflict()
        {
            // Arrange
            var productDto = new ProductDto { ProductId = "PROD123", Name = "Test Product" };
            var apiResponse = ApiResponseDto<ProductDto>.ErrorResponse("Product with ID PROD123 already exists");
            
            _mockProductService
                .Setup(service => service.CreateProductAsync(It.IsAny<ProductDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(conflictResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var productId = "PROD123";
            var productDto = new ProductDto { ProductId = productId, Name = "Updated Product" };
            var apiResponse = ApiResponseDto<ProductDto>.SuccessResponse(productDto);
            
            _mockProductService
                .Setup(service => service.UpdateProductAsync(productId, It.IsAny<ProductDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateProduct(productId, productDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.Name.Should().Be("Updated Product");
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = "INVALID_ID";
            var productDto = new ProductDto { ProductId = productId, Name = "Test Product" };
            var apiResponse = ApiResponseDto<ProductDto>.ErrorResponse($"Product with ID {productId} not found");
            
            _mockProductService
                .Setup(service => service.UpdateProductAsync(productId, It.IsAny<ProductDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateProduct(productId, productDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<ProductDto>>(notFoundResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task CreateProductsBatch_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var batchRequest = new BatchRequestDto<ProductDto>
            {
                RequestId = "BATCH123",
                Items = new List<ProductDto>
                {
                    new ProductDto { ProductId = "PROD1", Name = "Product 1" },
                    new ProductDto { ProductId = "PROD2", Name = "Product 2" }
                }
            };
            
            var apiResponse = ApiResponseDto<string>.SuccessResponse("BATCH123", "All 2 products processed successfully");
            
            _mockProductService
                .Setup(service => service.CreateProductsBatchAsync(It.IsAny<BatchRequestDto<ProductDto>>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateProductsBatch(batchRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<string>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().Be("BATCH123");
            returnValue.Message.Should().Contain("processed successfully");
        }
    }
}
