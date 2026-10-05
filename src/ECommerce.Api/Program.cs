using ECommerce.Api.Common.Endpoints;
using ECommerce.Api.Common.Messaging;
using ECommerce.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("ecommerce");
builder.Services.AddCqrs(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache(options =>
{
});



var app = builder.Build();


var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();



app.MapEndpoints();
app.MapDefaultEndpoints();
if(app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
