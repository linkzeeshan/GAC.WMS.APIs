using FluentAssertions;
using GAC.WMS.Integrations.API.Controllers;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GAC.WMS.Integrations.UnitTests.Controllers
{
    public class SalesOrdersControllerTests
    {
        private readonly Mock<ISalesOrderService> _mockSalesOrderService;
        private readonly Mock<ILogger<SalesOrdersController>> _mockLogger;
        private readonly SalesOrdersController _controller;

        public SalesOrdersControllerTests()
        {
            _mockSalesOrderService = new Mock<ISalesOrderService>();
            _mockLogger = new Mock<ILogger<SalesOrdersController>>();
            _controller = new SalesOrdersController(_mockSalesOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetSalesOrder_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var SONumber = "SO123";
            var salesOrderDto = new SalesOrderDto { SONumber = SONumber, CustomerId = "CUST123" };
            var apiResponse = ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto);
            
            _mockSalesOrderService
                .Setup(service => service.GetSalesOrderByNumberAsync(SONumber))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetSalesOrder(SONumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(okResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.SONumber.Should().Be(SONumber);
        }

        [Fact]
        public async Task GetSalesOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var SONumber = "INVALID_ID";
            var apiResponse = ApiResponseDto<SalesOrderDto>.ErrorResponse($"Sales order with ID {SONumber} not found");
            
            _mockSalesOrderService
                .Setup(service => service.GetSalesOrderByNumberAsync(SONumber))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetSalesOrder(SONumber);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(notFoundResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain(SONumber);
        }

        [Fact]
        public async Task CreateSalesOrder_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var salesOrderDto = new SalesOrderDto { SONumber = "SO123", CustomerId = "CUST123" };
            var apiResponse = ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto);
            
            _mockSalesOrderService
                .Setup(service => service.CreateSalesOrderAsync(It.IsAny<SalesOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(createdAtActionResult.Value);
            
            createdAtActionResult.ActionName.Should().Be(nameof(SalesOrdersController.GetSalesOrder));
            createdAtActionResult.RouteValues["SONumber"].Should().Be(salesOrderDto.SONumber);
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateSalesOrder_WithDuplicateId_ReturnsConflict()
        {
            // Arrange
            var salesOrderDto = new SalesOrderDto { SONumber = "SO123", CustomerId = "CUST123" };
            var apiResponse = ApiResponseDto<SalesOrderDto>.ErrorResponse("Sales order with ID SO123 already exists");
            
            _mockSalesOrderService
                .Setup(service => service.CreateSalesOrderAsync(It.IsAny<SalesOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateSalesOrder(salesOrderDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(conflictResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdateSalesOrder_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var SONumber = "SO123";
            var salesOrderDto = new SalesOrderDto { SONumber = SONumber, CustomerId = "CUST123" };
            var apiResponse = ApiResponseDto<SalesOrderDto>.SuccessResponse(salesOrderDto);
            
            _mockSalesOrderService
                .Setup(service => service.UpdateSalesOrderAsync(SONumber, It.IsAny<SalesOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateSalesOrder(SONumber, salesOrderDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.SONumber.Should().Be(SONumber);
        }

        [Fact]
        public async Task UpdateSalesOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var SONumber = "INVALID_ID";
            var salesOrderDto = new SalesOrderDto { SONumber = SONumber, CustomerId = "CUST123" };
            var apiResponse = ApiResponseDto<SalesOrderDto>.ErrorResponse($"Sales order with ID {SONumber} not found");
            
            _mockSalesOrderService
                .Setup(service => service.UpdateSalesOrderAsync(SONumber, It.IsAny<SalesOrderDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateSalesOrder(SONumber, salesOrderDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<SalesOrderDto>>(notFoundResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task CreateSalesOrdersBatch_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var batchRequest = new BatchRequestDto<SalesOrderDto>
            {
                RequestId = "BATCH123",
                Items = new List<SalesOrderDto>
                {
                    new SalesOrderDto { SONumber = "SO1", CustomerId = "CUST1" },
                    new SalesOrderDto { SONumber = "SO2", CustomerId = "CUST2" }
                }
            };
            
            var apiResponse = ApiResponseDto<string>.SuccessResponse("BATCH123", "All 2 sales orders processed successfully");
            
            _mockSalesOrderService
                .Setup(service => service.CreateSalesOrdersBatchAsync(It.IsAny<BatchRequestDto<SalesOrderDto>>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateSalesOrdersBatch(batchRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<string>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().Be("BATCH123");
            returnValue.Message.Should().Contain("processed successfully");
        }
    }
}
