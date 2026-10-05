using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;
using ECommerce.Api.Features.Products.CreateProduct;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Tests.Common;

public sealed class TestApplication : IAsyncDisposable
{
	private readonly WebApplication _app;

	private TestApplication(WebApplication app, HttpClient client)
	{
		_app = app;
		Client = client;
	}

	public HttpClient Client { get; }

	public static async Task<TestApplication> CreateAsync(CancellationToken ct = default)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Logging.ClearProviders();

		InMemoryDatabaseRoot databaseRoot = new();
		string databaseName = $"endpoint-tests-{Guid.NewGuid()}";

		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase(databaseName, databaseRoot));
		builder.Services.AddCqrs(typeof(CreateProductHandler).Assembly);
		builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
		builder.Services.AddMemoryCache();
		builder.Services.AddOpenApi();

		WebApplication app = builder.Build();
		app.MapEndpoints();
		app.MapOpenApi();
		await app.StartAsync(ct);

		return new TestApplication(app, app.GetTestClient());
	}

	public async Task ExecuteDbContextAsync(
		Func<AppDbContext, Task> action)
	{
		ArgumentNullException.ThrowIfNull(action);

		await using AsyncServiceScope scope = _app.Services.CreateAsyncScope();
		AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		await action(db);
	}

	public async ValueTask DisposeAsync()
	{
		Client.Dispose();
		await _app.DisposeAsync();
	}
}
