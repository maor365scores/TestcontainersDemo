using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Customers.Tests;

public sealed class CustomerServiceTest : IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
													 .WithImage("postgres:15-alpine")
													 .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
													 .Build();

	public Task InitializeAsync()
	{
		return _postgres.StartAsync();
	}

	public Task DisposeAsync()
	{
		return _postgres.DisposeAsync().AsTask();
	}
	

	[Fact]
	public void ShouldReturnTwoCustomers()
	{
		// Given
		var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));

		// When
		customerService.Create(new Customer(1, "George"));
		customerService.Create(new Customer(2, "John"));
		var customers = customerService.GetCustomers();

		// Then
		Assert.Equal(2, customers.Count());
	}

	[Fact]
	public void ShouldReturnCustomerById()
	{
		// Given
		var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));
		customerService.Create(new Customer(1, "George"));
		customerService.Create(new Customer(2, "John"));

		// When
		var customer = customerService.GetCustomerById(2);

		// Then
		Assert.NotNull(customer);
		Assert.Equal(2, customer?.Id);
		Assert.Equal("John", customer?.Name);
	}

	[Fact]
	public void ShouldReturnNullForNonExistentCustomer()
	{
		// Given
		var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));

		// When
		var customer = customerService.GetCustomerById(999); // Assuming 999 is an ID that does not exist

		// Then
		Assert.Null(customer);
	}
	
	[Fact]
	public void ShouldNotAllowDuplicateCustomerCreation()
	{
		// Arrange
		var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));
		customerService.Create(new Customer(1, "George"));

		// Act
		Action act = () => customerService.Create(new Customer(1, "George"));

		// Assert
		act.Should().Throw<NpgsqlException>()
		   .Where(e => e.SqlState == "23505")
		   .WithMessage("23505: duplicate key value violates unique constraint \"customers_pkey\"*");
	}
	
	[Fact]
	public void ShouldUpdateCustomerSuccessfully()
	{
		var customerService = new CustomerService(new DbConnectionProvider(_postgres.GetConnectionString()));
		customerService.Create(new Customer(1, "George"));

		customerService.Update(new Customer(1, "George Michael"));

		var updatedCustomer = customerService.GetCustomerById(1);
		Assert.NotNull(updatedCustomer);
		Assert.Equal("George Michael", updatedCustomer?.Name);
	}

}