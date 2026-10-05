using ECommerce.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Tests.Common;

public static class TestDbContextFactory
{
	public static AppDbContext Create()
	{
		DbContextOptions<AppDbContext> options =
			new DbContextOptionsBuilder<AppDbContext>()
				.UseInMemoryDatabase($"handler-tests-{Guid.NewGuid()}")
				.Options;

		return new AppDbContext(options);
	}
}
