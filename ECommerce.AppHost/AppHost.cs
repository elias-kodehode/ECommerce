using Google.Protobuf.WellKnownTypes;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb()
    .AddDatabase("ecommerce");

var api = builder.AddProject<Projects.ECommerce_Api>("backend")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");


var frontend = builder
   .AddViteApp("frontend", "../src/ECommerce.Web")
   .WithHttpEndpoint(port: 54131, name: "http")
   .WithReference(api)
   .WaitFor(api);

builder.Build().Run();
