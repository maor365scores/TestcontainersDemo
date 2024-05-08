using Testcontainers.Redis;

namespace CustomerService.Tests;

public class RedisServiceTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();

    public Task InitializeAsync() => _redisContainer.StartAsync();

    public Task DisposeAsync() => _redisContainer.DisposeAsync().AsTask();

    [Fact]
    public void SetAndGet_StringValue_ShouldReturnSameValue()
    {
        var redisService = new RedisService(_redisContainer.GetConnectionString());
        const string key = "testKey";
        const string value = "testValue";

        redisService.Set(key, value);
        var retrievedValue = redisService.Get<string>(key);

        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public void SetAndGet_DateTimeValue_ShouldReturnSameValue()
    {
        var redisService = new RedisService(_redisContainer.GetConnectionString());
        const string key = "dateTimeKey";
        var currentDateTime = DateTime.UtcNow;

        redisService.Set(key, currentDateTime);
        var retrievedDateTime = redisService.Get<DateTime>(key);

        Assert.Equal(currentDateTime, retrievedDateTime);
    }

    [Fact]
    public void SetAndGet_ComplexDataObject_ShouldReturnSameObject()
    {
        var redisService = new RedisService(_redisContainer.GetConnectionString());
        const string key = "complexData";

        var complexData = new ComplexData
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Age = 30,
            Emails = new List<string> { "john@example.com", "jdoe@example.org" },
            Addresses = new List<Address>
            {
                new()
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    ZipCode = "12345"
                },
                new()
                {
                    Street = "456 Oak Ln",
                    City = "Elsewhere",
                    State = "NY",
                    ZipCode = "67890"
                }
            },
            CreatedAt = DateTime.UtcNow
        };

        redisService.Set(key, complexData);
        var retrievedData = redisService.Get<ComplexData>(key);

        Assert.Equal(complexData.Id, retrievedData.Id);
        Assert.Equal(complexData.Name, retrievedData.Name);
        Assert.Equal(complexData.Age, retrievedData.Age);
        Assert.Equal(complexData.Emails, retrievedData.Emails);
        Assert.Equal(complexData.Addresses.Count, retrievedData.Addresses.Count);
        Assert.Equal(complexData.CreatedAt, retrievedData.CreatedAt);
    }

    private class ComplexData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<string> Emails { get; set; }
        public List<Address> Addresses { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}