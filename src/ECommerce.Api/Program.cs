using ECommerce.Api;
using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.OpenApi;
using ECommerce.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("ecommerce");
builder.Services.AddApiServices();

var app = builder.Build();

await app.MigrateDatabaseAsync();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.MapDefaultEndpoints();
app.MapApiDocumentation();

app.Run();
