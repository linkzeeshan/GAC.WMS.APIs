using AutoMapper;
using FluentAssertions;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
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
    public class PurchaseOrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<PurchaseOrderService>> _mockLogger;
        private readonly Mock<IRepository<PurchaseOrder, int>> _mockPurchaseOrderRepository;
        private readonly Mock<IRepository<ErrorLog, int>> _mockErrorLogRepository;
        private readonly Mock<IRepository<IntegrationLog, int>> _mockIntegrationLogRepository;
        private readonly PurchaseOrderService _purchaseOrderService;

        public PurchaseOrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<PurchaseOrderService>>();
            _mockPurchaseOrderRepository = new Mock<IRepository<PurchaseOrder, int>>();
            _mockErrorLogRepository = new Mock<IRepository<ErrorLog, int>>();
            _mockIntegrationLogRepository = new Mock<IRepository<IntegrationLog, int>>();

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<PurchaseOrder, int>())
                .Returns(_mockPurchaseOrderRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<ErrorLog, int>())
                .Returns(_mockErrorLogRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IntegrationLog, int>())
                .Returns(_mockIntegrationLogRepository.Object);

            _purchaseOrderService = new PurchaseOrderService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetPurchaseOrderByNumberAsync_WithExistingId_ReturnsPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = "PO123";
            var purchaseOrder = new PurchaseOrder { Id = 1, PONumber = purchaseOrderId, CustomerId = 1 };
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };

            var queryable = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<PurchaseOrderDto>(It.IsAny<PurchaseOrder>()))
                .Returns(purchaseOrderDto);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderByNumberAsync(purchaseOrderId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.PONumber.Should().Be(purchaseOrderId);

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPurchaseOrderByNumberAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var purchaseOrderId = "NONEXISTENT";
            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since purchase order doesn't exist
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderByNumberAsync(purchaseOrderId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseOrderAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = "PO123", CustomerId = 1 };
            var purchaseOrder = new PurchaseOrder { Id = 1, PONumber = "PO123", CustomerId = 1 };
            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since purchase order doesn't exist yet
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<PurchaseOrder>(It.IsAny<PurchaseOrderDto>()))
                .Returns(purchaseOrder);

            _mockPurchaseOrderRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<PurchaseOrder>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _purchaseOrderService.CreatePurchaseOrderAsync(purchaseOrderDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.PONumber.Should().Be(purchaseOrderDto.PONumber);
            result.Message.Should().Contain("created successfully");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<PurchaseOrder>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreatePurchaseOrderAsync_WithExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = "PO123", CustomerId = 1 };
            var existingPurchaseOrder = new PurchaseOrder { Id = 1, PONumber = "PO123", CustomerId = 1 };
            var purchaseOrders = new List<PurchaseOrder> { existingPurchaseOrder };
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _purchaseOrderService.CreatePurchaseOrderAsync(purchaseOrderDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("already exists");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<PurchaseOrder>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdatePurchaseOrderAsync_WithExistingId_ReturnsSuccessResponse()
        {
            // Arrange
            var purchaseOrderId = "PO123";
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };
            var existingPurchaseOrder = new PurchaseOrder { Id = 1, PONumber = purchaseOrderId, CustomerId = 1 };
            var purchaseOrders = new List<PurchaseOrder> { existingPurchaseOrder };
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map(It.IsAny<PurchaseOrderDto>(), It.IsAny<PurchaseOrder>()))
                .Callback<PurchaseOrderDto, PurchaseOrder>((dto, entity) => {
                    entity.CustomerId = dto.CustomerId;
                });

            _mockPurchaseOrderRepository
                .Setup(repo => repo.Update(It.IsAny<PurchaseOrder>()));

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _purchaseOrderService.UpdatePurchaseOrderAsync(purchaseOrderId, purchaseOrderDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Message.Should().Contain("updated successfully");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.Update(It.IsAny<PurchaseOrder>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePurchaseOrderAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var purchaseOrderId = "NONEXISTENT";
            var purchaseOrderDto = new PurchaseOrderDto { PONumber = purchaseOrderId, CustomerId = 1 };
            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since purchase order doesn't exist
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _purchaseOrderService.UpdatePurchaseOrderAsync(purchaseOrderId, purchaseOrderDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.Update(It.IsAny<PurchaseOrder>()),
                Times.Never);
        }

        [Fact]
        public async Task CreatePurchaseOrdersBatchAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var batchRequest = new BatchRequestDto<PurchaseOrderDto>
            {
                RequestId = "BATCH123",
                Items = new List<PurchaseOrderDto>
                {
                    new PurchaseOrderDto { PONumber = "PO1", CustomerId = 1 },
                    new PurchaseOrderDto { PONumber = "PO2", CustomerId = 1 }
                }
            };

            var purchaseOrder1 = new PurchaseOrder { Id = 1, PONumber = "PO1", CustomerId = 1 };
            var purchaseOrder2 = new PurchaseOrder { Id = 2, PONumber = "PO2", CustomerId = 1 };

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since purchase orders don't exist yet
            var emptyQueryable = purchaseOrders.AsQueryable();
            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(emptyQueryable);

            _mockMapper
                .Setup(mapper => mapper.Map<PurchaseOrder>(It.Is<PurchaseOrderDto>(dto => dto.PONumber == "PO1")))
                .Returns(purchaseOrder1);

            _mockMapper
                .Setup(mapper => mapper.Map<PurchaseOrder>(It.Is<PurchaseOrderDto>(dto => dto.PONumber == "PO2")))
                .Returns(purchaseOrder2);

            _mockPurchaseOrderRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<PurchaseOrder>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _purchaseOrderService.CreatePurchaseOrdersBatchAsync(batchRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().Contain(batchRequest.RequestId);
            result.Message.Should().Contain("processed successfully");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<PurchaseOrder>()),
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

        [Fact]
        public async Task GetPurchaseOrderPONumberAsync_WithExistingPONumber_ReturnsListOfPurchaseOrders()
        {
            // Arrange
            var poNumber = "PO123";
            var purchaseOrder1 = new PurchaseOrder { Id = 1, PONumber = poNumber, CustomerId = 1 };
            var purchaseOrder2 = new PurchaseOrder { Id = 2, PONumber = poNumber, CustomerId = 2 };
            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder1, purchaseOrder2 };
            
            var purchaseOrderDto1 = new PurchaseOrderDto { PONumber = poNumber, CustomerId = 1 };
            var purchaseOrderDto2 = new PurchaseOrderDto { PONumber = poNumber, CustomerId = 2 };
            var purchaseOrderDtos = new List<PurchaseOrderDto> { purchaseOrderDto1, purchaseOrderDto2 };

            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<List<PurchaseOrderDto>>(It.IsAny<List<PurchaseOrder>>()))
                .Returns(purchaseOrderDtos);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderPONumberAsync(poNumber);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data[0].PONumber.Should().Be(poNumber);
            result.Data[1].PONumber.Should().Be(poNumber);

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPurchaseOrderPONumberAsync_WithNonExistingPONumber_ReturnsErrorResponse()
        {
            // Arrange
            var poNumber = "NONEXISTENT";
            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since purchase orders don't exist
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderPONumberAsync(poNumber);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPurchaseOrderByIdAsync_WithExistingCustomerId_ReturnsListOfPurchaseOrders()
        {
            // Arrange
            var customerId = 1;
            var customerIdStr = customerId.ToString();
            var purchaseOrder1 = new PurchaseOrder { Id = 1, PONumber = "PO123", CustomerId = customerId };
            var purchaseOrder2 = new PurchaseOrder { Id = 2, PONumber = "PO456", CustomerId = customerId };
            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder1, purchaseOrder2 };
            
            var purchaseOrderDto1 = new PurchaseOrderDto { PONumber = "PO123", CustomerId = customerId };
            var purchaseOrderDto2 = new PurchaseOrderDto { PONumber = "PO456", CustomerId = customerId };
            var purchaseOrderDtos = new List<PurchaseOrderDto> { purchaseOrderDto1, purchaseOrderDto2 };

            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<List<PurchaseOrderDto>>(It.IsAny<List<PurchaseOrder>>()))
                .Returns(purchaseOrderDtos);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderByIdAsync(customerIdStr);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.All(po => po.CustomerId == customerId).Should().BeTrue();

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPurchaseOrderByIdAsync_WithNonExistingCustomerId_ReturnsErrorResponse()
        {
            // Arrange
            var customerId = "999"; // Non-existent customer ID
            var purchaseOrders = new List<PurchaseOrder>(); // Empty list since no purchase orders exist for this customer
            var queryable = purchaseOrders.AsQueryable();

            _mockPurchaseOrderRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _purchaseOrderService.GetPurchaseOrderByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockPurchaseOrderRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()),
                Times.Once);
        }
    }
}
