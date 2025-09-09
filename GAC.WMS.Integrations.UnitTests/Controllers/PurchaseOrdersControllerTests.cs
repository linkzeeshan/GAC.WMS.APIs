using FluentAssertions;
using GAC.WMS.Integrations.API.Controllers;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GAC.WMS.Integrations.UnitTests.Controllers
{
    public class PurchaseOrdersControllerTests
    {
        private readonly Mock<IPurchaseOrderService> _mockPurchaseOrderService;
        private readonly Mock<ILogger<PurchaseOrdersController>> _mockLogger;
        private readonly PurchaseOrdersController _controller;

        public PurchaseOrdersControllerTests()
        {
            _mockPurchaseOrderService = new Mock<IPurchaseOrderService>();
            _mockLogger = new Mock<ILogger<PurchaseOrdersController>>();
            _controller = new PurchaseOrdersController(_mockPurchaseOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetPurchaseOrder_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var purchaseOrderId = "PO123";
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto);
            
            _mockPurchaseOrderService
                .Setup(service => service.GetPurchaseOrderByNumberAsync(purchaseOrderId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetPurchaseOrder(purchaseOrderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(okResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.PONumber.Should().Be(purchaseOrderId);
        }

        [Fact]
        public async Task GetPurchaseOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var purchaseOrderId = "INVALID_ID";
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.ErrorResponse($"Purchase order with ID {purchaseOrderId} not found");
            
            _mockPurchaseOrderService
                .Setup(service => service.GetPurchaseOrderByNumberAsync(purchaseOrderId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetPurchaseOrder(purchaseOrderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(notFoundResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain(purchaseOrderId);
        }

        [Fact]
        public async Task CreatePurchaseOrder_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = "PO123", CustomerId = 1 };
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto);
            
            _mockPurchaseOrderService
                .Setup(service => service.CreatePurchaseOrderAsync(It.IsAny<PurchaseOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(createdAtActionResult.Value);
            
            createdAtActionResult.ActionName.Should().Be(nameof(PurchaseOrdersController.GetPurchaseOrder));
            createdAtActionResult.RouteValues["purchaseOrderId"].Should().Be(purchaseOrderDto.PONumber);
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreatePurchaseOrder_WithDuplicateId_ReturnsConflict()
        {
            // Arrange
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = "PO123", CustomerId = 1 };
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.ErrorResponse("Purchase order with ID PO123 already exists");
            
            _mockPurchaseOrderService
                .Setup(service => service.CreatePurchaseOrderAsync(It.IsAny<PurchaseOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreatePurchaseOrder(purchaseOrderDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(conflictResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdatePurchaseOrder_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var purchaseOrderId = "PO123";
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.SuccessResponse(purchaseOrderDto);
            
            _mockPurchaseOrderService
                .Setup(service => service.UpdatePurchaseOrderAsync(purchaseOrderId, It.IsAny<PurchaseOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdatePurchaseOrder(purchaseOrderId, purchaseOrderDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.PONumber.Should().Be(purchaseOrderId);
        }

        [Fact]
        public async Task UpdatePurchaseOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var purchaseOrderId = "INVALID_ID";
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };
            var apiResponse = ApiResponseDto<PurchaseOrderDto>.ErrorResponse($"Purchase order with ID {purchaseOrderId} not found");
            
            _mockPurchaseOrderService
                .Setup(service => service.UpdatePurchaseOrderAsync(purchaseOrderId, It.IsAny<PurchaseOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdatePurchaseOrder(purchaseOrderId, purchaseOrderDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<PurchaseOrderDto>>(notFoundResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task CreatePurchaseOrdersBatch_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var batchRequest = new BatchRequestDto<PurchaseOrderDto>
            {
                RequestId = "BATCH123",
                Items = new List<PurchaseOrderDto>
                {
                    new PurchaseOrderDto { PONumber = "PO1", CustomerId = 1 },
                    new PurchaseOrderDto {PONumber = "PO2", CustomerId = 1 }
                }
            };
            
            var apiResponse = ApiResponseDto<string>.SuccessResponse("BATCH123", "All 2 purchase orders processed successfully");
            
            _mockPurchaseOrderService
                .Setup(service => service.CreatePurchaseOrdersBatchAsync(It.IsAny<BatchRequestDto<PurchaseOrderDto>>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreatePurchaseOrdersBatch(batchRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<string>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().Be("BATCH123");
            returnValue.Message.Should().Contain("processed successfully");
        }
    }
}
