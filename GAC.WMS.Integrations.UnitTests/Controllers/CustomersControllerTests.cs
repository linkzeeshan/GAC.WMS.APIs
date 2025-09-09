using FluentAssertions;
using GAC.WMS.Integrations.API.Controllers;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
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
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<ILogger<CustomersController>> _mockLogger;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockLogger = new Mock<ILogger<CustomersController>>();
            _controller = new CustomersController(_mockCustomerService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCustomer_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var customerId = "CUST123";
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Test Customer" };
            var apiResponse = ApiResponseDto<CustomerDto>.SuccessResponse(customerDto);
            
            _mockCustomerService
                .Setup(service => service.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetCustomer(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(okResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.CustomerId.Should().Be(customerId);
        }

        [Fact]
        public async Task GetCustomer_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var customerId = "INVALID_ID";
            var apiResponse = ApiResponseDto<CustomerDto>.ErrorResponse($"Customer with ID {customerId} not found");
            
            _mockCustomerService
                .Setup(service => service.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetCustomer(customerId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(notFoundResult.Value);
            
            returnValue.Should().NotBeNull();
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain(customerId);
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var customerDto = new CustomerDto { CustomerId = "CUST123", Name = "Test Customer" };
            var apiResponse = ApiResponseDto<CustomerDto>.SuccessResponse(customerDto);
            
            _mockCustomerService
                .Setup(service => service.CreateCustomerAsync(It.IsAny<CustomerDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateCustomer(customerDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(createdAtActionResult.Value);
            
            createdAtActionResult.ActionName.Should().Be(nameof(CustomersController.GetCustomer));
            createdAtActionResult.RouteValues["customerId"].Should().Be(customerDto.CustomerId);
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCustomer_WithDuplicateId_ReturnsConflict()
        {
            // Arrange
            var customerDto = new CustomerDto { CustomerId = "CUST123", Name = "Test Customer" };
            var apiResponse = ApiResponseDto<CustomerDto>.ErrorResponse("Customer with ID CUST123 already exists");
            
            _mockCustomerService
                .Setup(service => service.CreateCustomerAsync(It.IsAny<CustomerDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateCustomer(customerDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(conflictResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdateCustomer_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var customerId = "CUST123";
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Updated Customer" };
            var apiResponse = ApiResponseDto<CustomerDto>.SuccessResponse(customerDto);
            
            _mockCustomerService
                .Setup(service => service.UpdateCustomerAsync(customerId, It.IsAny<CustomerDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateCustomer(customerId, customerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().NotBeNull();
            returnValue.Data.Name.Should().Be("Updated Customer");
        }

        [Fact]
        public async Task UpdateCustomer_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var customerId = "INVALID_ID";
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Test Customer" };
            var apiResponse = ApiResponseDto<CustomerDto>.ErrorResponse($"Customer with ID {customerId} not found");
            
            _mockCustomerService
                .Setup(service => service.UpdateCustomerAsync(customerId, It.IsAny<CustomerDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.UpdateCustomer(customerId, customerDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<CustomerDto>>(notFoundResult.Value);
            
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task CreateCustomersBatch_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var batchRequest = new BatchRequestDto<CustomerDto>
            {
                RequestId = "BATCH123",
                Items = new List<CustomerDto>
                {
                    new CustomerDto { CustomerId = "CUST1", Name = "Customer 1" },
                    new CustomerDto { CustomerId = "CUST2", Name = "Customer 2" }
                }
            };
            
            var apiResponse = ApiResponseDto<string>.SuccessResponse("BATCH123", "All 2 customers processed successfully");
            
            _mockCustomerService
                .Setup(service => service.CreateCustomersBatchAsync(It.IsAny<BatchRequestDto<CustomerDto>>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.CreateCustomersBatch(batchRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponseDto<string>>(okResult.Value);
            
            returnValue.Success.Should().BeTrue();
            returnValue.Data.Should().Be("BATCH123");
            returnValue.Message.Should().Contain("processed successfully");
        }
    }
}
