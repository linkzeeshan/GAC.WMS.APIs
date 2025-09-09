using AutoMapper;
using FluentAssertions;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.Services.Implementation;
using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GAC.WMS.Integrations.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<CustomerService>> _mockLogger;
        private readonly Mock<IRepository<Customer, int>> _mockCustomerRepository;
        private readonly Mock<IRepository<ErrorLog, int>> _mockErrorLogRepository;
        private readonly Mock<IRepository<IntegrationLog, int>> _mockIntegrationLogRepository;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CustomerService>>();
            _mockCustomerRepository = new Mock<IRepository<Customer, int>>();
            _mockErrorLogRepository = new Mock<IRepository<ErrorLog, int>>();
            _mockIntegrationLogRepository = new Mock<IRepository<IntegrationLog, int>>();

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<Customer, int>())
                .Returns(_mockCustomerRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<ErrorLog, int>())
                .Returns(_mockErrorLogRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IntegrationLog, int>())
                .Returns(_mockIntegrationLogRepository.Object);

            _customerService = new CustomerService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithExistingId_ReturnsCustomer()
        {
            // Arrange
            var customerId = "CUST123";
            var customer = new Customer { Id = 1, CustomerId = customerId, Name = "Test Customer" };
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Test Customer" };

            // Create a queryable list of customers that can be used with LINQ
            var customers = new List<Customer> { customer };
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<CustomerDto>(It.IsAny<Customer>()))
                .Returns(customerDto);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.CustomerId.Should().Be(customerId);

            _mockCustomerRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var customerId = "NONEXISTENT";
            var customers = new List<Customer>(); // Empty list since customer doesn't exist
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockCustomerRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var customerDto = new CustomerDto { CustomerId = "CUST123", Name = "Test Customer" };
            var customer = new Customer { Id = 1, CustomerId = "CUST123", Name = "Test Customer" };
            var customers = new List<Customer>(); // Empty list since customer doesn't exist yet
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<Customer>(It.IsAny<CustomerDto>()))
                .Returns(customer);

            _mockCustomerRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _customerService.CreateCustomerAsync(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.CustomerId.Should().Be(customerDto.CustomerId);
            result.Message.Should().Contain("created successfully");

            _mockCustomerRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Customer>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WithExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var customerDto = new CustomerDto { CustomerId = "CUST123", Name = "Test Customer" };
            var existingCustomer = new Customer { Id = 1, CustomerId = "CUST123", Name = "Existing Customer" };
            var customers = new List<Customer> { existingCustomer };
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _customerService.CreateCustomerAsync(customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("already exists");

            _mockCustomerRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Customer>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WithExistingId_ReturnsSuccessResponse()
        {
            // Arrange
            var customerId = "CUST123";
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Updated Customer" };
            var existingCustomer = new Customer { Id = 1, CustomerId = customerId, Name = "Original Customer" };
            var customers = new List<Customer> { existingCustomer };
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map(It.IsAny<CustomerDto>(), It.IsAny<Customer>()))
                .Callback<CustomerDto, Customer>((dto, entity) => {
                    entity.Name = dto.Name;
                });

            _mockCustomerRepository
                .Setup(repo => repo.Update(It.IsAny<Customer>()));

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _customerService.UpdateCustomerAsync(customerId, customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Message.Should().Contain("updated successfully");

            _mockCustomerRepository.Verify(
                repo => repo.Update(It.IsAny<Customer>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var customerId = "NONEXISTENT";
            var customerDto = new CustomerDto { CustomerId = customerId, Name = "Updated Customer" };
            var customers = new List<Customer>(); // Empty list since customer doesn't exist
            var queryable = customers.AsQueryable();

            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _customerService.UpdateCustomerAsync(customerId, customerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockCustomerRepository.Verify(
                repo => repo.Update(It.IsAny<Customer>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateCustomersBatchAsync_WithValidData_ReturnsSuccessResponse()
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

            var customer1 = new Customer { Id = 1, CustomerId = "CUST1", Name = "Customer 1" };
            var customer2 = new Customer { Id = 2, CustomerId = "CUST2", Name = "Customer 2" };

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            var customers = new List<Customer>(); // Empty list since customers don't exist yet
            var emptyQueryable = customers.AsQueryable();
            _mockCustomerRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Customer, bool>>>()))
                .Returns(emptyQueryable);

            _mockMapper
                .Setup(mapper => mapper.Map<Customer>(It.Is<CustomerDto>(dto => dto.CustomerId == "CUST1")))
                .Returns(customer1);

            _mockMapper
                .Setup(mapper => mapper.Map<Customer>(It.Is<CustomerDto>(dto => dto.CustomerId == "CUST2")))
                .Returns(customer2);

            _mockCustomerRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.CreateCustomersBatchAsync(batchRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().Contain(batchRequest.RequestId);
            result.Message.Should().Contain("processed successfully");

            _mockCustomerRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Customer>()),
                Times.Exactly(2));

            _mockUnitOfWork.Verify(
                uow => uow.BeginTransactionAsync(),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.CommitTransactionAsync(),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.RollbackTransactionAsync(),
                Times.Never);
        }
    }
}
