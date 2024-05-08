using Testcontainers.Redis;

namespace CustomerService.Tests;

public class RedisServiceTests : IAsyncLifetime
{
	private readonly RedisContainer _redisContainer = new RedisBuilder().Build();

	public Task InitializeAsync() => _redisContainer.StartAsync();

	public Task DisposeAsync() => _redisContainer.DisposeAsync().AsTask();

	[Fact]
	public void SetAndGetValueFromRedis()
	{
		var redisService = new RedisService(_redisContainer.GetConnectionString());
		const string key = "testKey";
		const string value = "testValue";

		redisService.Set(key, value);
		var retrievedValue = redisService.Get<string>(key);

		Assert.Equal(value, retrievedValue);
	}

	[Fact]
	public void SetAndGetDateTimeFromRedis()
	{
		var redisService = new RedisService(_redisContainer.GetConnectionString());
		const string key = "dateTimeKey";
		var currentDateTime = DateTime.UtcNow;

		redisService.Set(key, currentDateTime);
		var retrievedDateTime = redisService.Get<DateTime>(key);

		Assert.Equal(currentDateTime, retrievedDateTime);
	}
}