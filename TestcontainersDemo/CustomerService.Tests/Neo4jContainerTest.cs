using Neo4j.Driver;
using Testcontainers.Neo4j;

namespace CustomerService.Tests;

public sealed class Neo4jContainerTest : IAsyncLifetime
{
	private readonly Neo4jContainer _neo4JContainer
		= new Neo4jBuilder().Build();

	[Fact]
	public async Task CanReadNeo4jDatabase()
	{
		const string database = "neo4j";

		await using var client = GraphDatabase.Driver(_neo4JContainer.GetConnectionString());

		await using var session = client.AsyncSession(cfg => cfg.WithDatabase(database));

		Assert.Equal(database, session.SessionConfig.Database);
	}

	[Fact]
	public async Task CanWriteAndReadData()
	{
		const string database = "neo4j";
		const string testNodeName = "TestNode";

		await using var client = GraphDatabase.Driver(_neo4JContainer.GetConnectionString());
		await using var session = client.AsyncSession(o => o.WithDatabase(database));

		// Write data to Neo4j
		var writeResult = await session.ExecuteWriteAsync(async transaction =>
		{
			var createNodeQuery = $"CREATE (n:Node {{ name: '{testNodeName}' }}) RETURN n";
			var result = await transaction.RunAsync(createNodeQuery);
			return await result.SingleAsync();
		});

		Assert.NotNull(writeResult);
		Assert.Equal(testNodeName, writeResult["n"].As<INode>().Properties["name"].As<string>());

		// Read data from Neo4j
		var readResult = await session.ExecuteReadAsync(async transaction =>
		{
			var readNodeQuery = $"MATCH (n:Node {{ name: '{testNodeName}' }}) RETURN n";
			var result = await transaction.RunAsync(readNodeQuery);
			return await result.SingleAsync();
		});

		Assert.NotNull(readResult);
		Assert.Equal(testNodeName, readResult["n"].As<INode>().Properties["name"].As<string>());
	}

	public Task InitializeAsync()
		=> _neo4JContainer.StartAsync();

	public Task DisposeAsync()
		=> _neo4JContainer.DisposeAsync().AsTask();
}