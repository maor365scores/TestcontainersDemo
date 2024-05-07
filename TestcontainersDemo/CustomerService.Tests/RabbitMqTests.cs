using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.RabbitMq;

namespace CustomerService.Tests;

public sealed class RabbitMqContainerTest : IAsyncLifetime
{
	private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder().Build();

	public Task InitializeAsync()
	{
		return _rabbitMqContainer.StartAsync();
	}

	public Task DisposeAsync()
	{
		return _rabbitMqContainer.DisposeAsync().AsTask();
	}

	[Fact]
	public void ConsumeMessageFromQueue()
	{
		const string queue = "hello";

		const string message = "Hello World!";

		string actualMessage = null;

		// Signal the completion of message reception.
		EventWaitHandle waitHandle = new ManualResetEvent(false);

		// Create and establish a connection.
		var connectionFactory = new ConnectionFactory
		{
			Uri = new Uri(_rabbitMqContainer.GetConnectionString())
		};
		using var connection = connectionFactory.CreateConnection();

		// Send a message to the channel.
		using var channel = connection.CreateModel();
		channel.QueueDeclare(queue, false, false, false, null);
		channel.BasicPublish(string.Empty, queue, null, Encoding.Default.GetBytes(message));

		// Consume a message from the channel.
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (_, eventArgs) =>
		{
			actualMessage = Encoding.Default.GetString(eventArgs.Body.ToArray());
			waitHandle.Set();
		};

		channel.BasicConsume(queue, true, consumer);
		waitHandle.WaitOne(TimeSpan.FromSeconds(1));

		Assert.Equal(message, actualMessage);
	}
}