using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Data;

public static class DatabaseExtensions
{
    public static async Task MigrateDatabaseAsync(
        this WebApplication app,
        CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);
    }
}
