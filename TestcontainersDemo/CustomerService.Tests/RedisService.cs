using System.Text.Json;

namespace CustomerService.Tests;

using StackExchange.Redis;

public class RedisService
{
	private readonly ConnectionMultiplexer _redis;

	public RedisService(string connectionString)
	{
		_redis = ConnectionMultiplexer.Connect(connectionString);
	}

	public void Set<T>(string key, T value)
	{
		var db = _redis.GetDatabase();
		var serializedValue = JsonSerializer.Serialize(value);
		db.StringSet(key, serializedValue);
	}
	
	public T Get<T>(string key)
	{
		var db = _redis.GetDatabase();
		var value = db.StringGet(key);
		return JsonSerializer.Deserialize<T>(value);
	}
}