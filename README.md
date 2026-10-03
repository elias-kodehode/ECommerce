# ECommerce

.NET 10 API with Aspire, PostgreSQL, and Entity Framework Core.

## Run locally

Install the .NET 10 SDK and start Docker Desktop with Linux containers, then run:

```powershell
dotnet run --project ECommerce.AppHost
```

The AppHost starts PostgreSQL, creates the `ecommerce` database, and starts the API
once the database is ready. Aspire supplies the API connection string automatically;
database credentials are not stored in the repository. PostgreSQL data persists in a
Docker volume between runs.

Open the dashboard URL printed in the terminal to view the resources and API endpoints.
In development, the API exposes `/health` (including database connectivity), `/alive`,
and `/openapi/v1.json`.

`src/ECommerce.Api/Data/AppDbContext.cs` is registered with the PostgreSQL EF Core
provider. It is empty and ready for the shop's entities. The EF Core design-time
package is included for future migrations; no entities or migrations have been added.

If running the API directly or using EF CLI tools outside AppHost, supply
`ConnectionStrings__ecommerce` through an environment variable with the connection
string from the Aspire dashboard.
