using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Testcontainers.LocalStack;

namespace CustomerService.Tests;

public class AWSTests : IAsyncLifetime
{
	private readonly LocalStackContainer _container = new LocalStackBuilder()
		.Build();

	public Task InitializeAsync() => _container.StartAsync();

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();

	[Fact]
	public async Task UploadFileToS3_ShouldSucceed()
	{
		const string bucketName = "my-test-bucket";
		var s3Client = new AmazonS3Client(new AmazonS3Config
		{
			ServiceURL = _container.GetConnectionString()
		});

		// Create S3 bucket
		await s3Client.PutBucketAsync(bucketName);

		// Upload file to S3
		var fileBytes = System.Text.Encoding.UTF8.GetBytes("Hello, LocalStack!");
		var putObjectRequest = new PutObjectRequest
		{
			BucketName = bucketName,
			Key = "test.txt",
			InputStream = new MemoryStream(fileBytes)
		};
		await s3Client.PutObjectAsync(putObjectRequest);

		// Assert file exists in S3
		var getObjectResponse = await s3Client.GetObjectAsync(bucketName, "test.txt");
		var fileContent = await new StreamReader(getObjectResponse.ResponseStream).ReadToEndAsync();
		fileContent.Should().Be("Hello, LocalStack!");
	}
}