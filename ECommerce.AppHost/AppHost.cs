var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb()
    .AddDatabase("ecommerce");

builder.AddProject<Projects.ECommerce_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
