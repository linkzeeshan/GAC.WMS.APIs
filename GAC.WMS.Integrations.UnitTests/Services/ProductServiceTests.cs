using AutoMapper;
using FluentAssertions;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Products;
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
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly Mock<IRepository<Product, int>> _mockProductRepository;
        private readonly Mock<IRepository<ErrorLog, int>> _mockErrorLogRepository;
        private readonly Mock<IRepository<IntegrationLog, int>> _mockIntegrationLogRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _mockProductRepository = new Mock<IRepository<Product, int>>();
            _mockErrorLogRepository = new Mock<IRepository<ErrorLog, int>>();
            _mockIntegrationLogRepository = new Mock<IRepository<IntegrationLog, int>>();

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<Product, int>())
                .Returns(_mockProductRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<ErrorLog, int>())
                .Returns(_mockErrorLogRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IntegrationLog, int>())
                .Returns(_mockIntegrationLogRepository.Object);

            _productService = new ProductService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithExistingId_ReturnsProduct()
        {
            // Arrange
            var productId = "PROD123";
            var product = new Product { Id = 1, ProductId = productId, Name = "Test Product" };
            var productDto = new ProductDto { ProductId = productId, Name = "Test Product" };

            var mockDbSet = new Mock<DbSet<Product>>();
            var queryable = new List<Product> { product }.AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(productDto);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.ProductId.Should().Be(productId);

            _mockProductRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var productId = "NONEXISTENT";
            var queryable = new List<Product>().AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockProductRepository.Verify(
                repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var productDto = new ProductDto { ProductId = "PROD123", Name = "Test Product" };
            var product = new Product { Id = 1, ProductId = "PROD123", Name = "Test Product" };
            var queryable = new List<Product>().AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map<Product>(It.IsAny<ProductDto>()))
                .Returns(product);

            _mockProductRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _productService.CreateProductAsync(productDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.ProductId.Should().Be(productDto.ProductId);
            result.Message.Should().Contain("created successfully");

            _mockProductRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Product>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var productDto = new ProductDto { ProductId = "PROD123", Name = "Test Product" };
            var existingProduct = new Product { Id = 1, ProductId = "PROD123", Name = "Existing Product" };
            var queryable = new List<Product> { existingProduct }.AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _productService.CreateProductAsync(productDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("already exists");

            _mockProductRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Product>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_WithExistingId_ReturnsSuccessResponse()
        {
            // Arrange
            var productId = "PROD123";
            var productDto = new ProductDto { ProductId = productId, Name = "Updated Product" };
            var existingProduct = new Product { Id = 1, ProductId = productId, Name = "Original Product" };
            var queryable = new List<Product> { existingProduct }.AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            _mockMapper
                .Setup(mapper => mapper.Map(It.IsAny<ProductDto>(), It.IsAny<Product>()))
                .Callback<ProductDto, Product>((dto, entity) => {
                    entity.Name = dto.Name;
                });

            _mockProductRepository
                .Setup(repo => repo.Update(It.IsAny<Product>()));

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _productService.UpdateProductAsync(productId, productDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Message.Should().Contain("updated successfully");

            _mockProductRepository.Verify(
                repo => repo.Update(It.IsAny<Product>()),
                Times.Once);

            _mockIntegrationLogRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<IntegrationLog>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingId_ReturnsErrorResponse()
        {
            // Arrange
            var productId = "NONEXISTENT";
            var productDto = new ProductDto { ProductId = productId, Name = "Updated Product" };
            var queryable = new List<Product>().AsQueryable();

            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(queryable);

            // Act
            var result = await _productService.UpdateProductAsync(productId, productDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Contain("not found");

            _mockProductRepository.Verify(
                repo => repo.Update(It.IsAny<Product>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateProductsBatchAsync_WithValidData_ReturnsSuccessResponse()
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

            var product1 = new Product { Id = 1, ProductId = "PROD1", Name = "Product 1" };
            var product2 = new Product { Id = 2, ProductId = "PROD2", Name = "Product 2" };

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            var emptyQueryable = new List<Product>().AsQueryable();
            _mockProductRepository
                .Setup(repo => repo.GetByCondition(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(emptyQueryable);

            _mockMapper
                .Setup(mapper => mapper.Map<Product>(It.Is<ProductDto>(dto => dto.ProductId == "PROD1")))
                .Returns(product1);

            _mockMapper
                .Setup(mapper => mapper.Map<Product>(It.Is<ProductDto>(dto => dto.ProductId == "PROD2")))
                .Returns(product2);

            _mockProductRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockIntegrationLogRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<IntegrationLog>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateProductsBatchAsync(batchRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().Contain(batchRequest.RequestId);
            result.Message.Should().Contain("processed successfully");

            _mockProductRepository.Verify(
                repo => repo.CreateAsync(It.IsAny<Product>()),
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
