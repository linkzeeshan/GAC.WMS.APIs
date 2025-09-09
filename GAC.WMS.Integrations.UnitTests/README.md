# Unit Testing Guidelines for GAC.WMS.APIs

This document outlines the approach, patterns, and best practices for unit testing in the GAC.WMS.APIs project.

## Testing Framework and Tools

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for creating test doubles
- **FluentAssertions**: For more readable assertions
- **Microsoft.EntityFrameworkCore.InMemory**: For testing components that interact with the database

## Project Structure

The unit test project mirrors the structure of the main project:

```
GAC.WMS.Integrations.UnitTests/
├── Controllers/           # Tests for API controllers
├── Services/              # Tests for application services
└── Helpers/               # Test utilities and helpers
```

## Naming Conventions

- Test classes should be named `{ClassUnderTest}Tests`
- Test methods should follow the pattern `{MethodUnderTest}_{Scenario}_{ExpectedBehavior}`
  - Example: `GetCustomerByIdAsync_WithExistingId_ReturnsCustomer`

## Test Structure

Each test should follow the Arrange-Act-Assert (AAA) pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up the test data and conditions
    
    // Act - Call the method being tested
    
    // Assert - Verify the results
}
```

## Mocking Guidelines

1. Use `Mock<T>` to create mock objects for dependencies
2. Set up only the methods that will be called during the test
3. Verify important interactions with dependencies
4. Avoid excessive mocking - focus on the behavior, not the implementation

## Example Test

```csharp
[Fact]
public async Task GetCustomerByIdAsync_WithExistingId_ReturnsCustomer()
{
    // Arrange
    var customerId = "CUST123";
    var customer = new Customer { Id = 1, CustomerId = customerId, Name = "Test Customer" };
    var customerDto = new CustomerDto { CustomerId = customerId, Name = "Test Customer" };
    
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
```

## Test Helpers

We've created helper classes to simplify common testing tasks:

1. `TestHelper`: Provides utilities for creating mappers and loggers
2. `MockRepositoryHelper`: Simplifies setting up mock repositories and unit of work

## Best Practices

1. **Test in isolation**: Each test should focus on a single unit of functionality
2. **Use meaningful test data**: Make test data clear and relevant to the scenario
3. **Test edge cases**: Include tests for error conditions and boundary cases
4. **Keep tests independent**: Tests should not depend on each other
5. **Avoid test logic**: Minimize conditional logic in tests
6. **Test public API**: Focus on testing the public interface, not implementation details
7. **Maintain tests**: Update tests when the code changes

## Running Tests

To run all tests:

```
dotnet test GAC.WMS.Integrations.UnitTests
```

To run specific tests:

```
dotnet test GAC.WMS.Integrations.UnitTests --filter "FullyQualifiedName~CustomerServiceTests"
```

## Code Coverage

To generate code coverage reports:

```
dotnet test GAC.WMS.Integrations.UnitTests /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```
