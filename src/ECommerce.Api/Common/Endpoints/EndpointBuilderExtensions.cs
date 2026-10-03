namespace ECommerce.Api.Common.Endpoints;



public static class EndpointBuilderExtensions
{
	public static void MapEndpoints(this WebApplication app)
	{
		var logger = app.Services.GetRequiredService<ILogger<IEndpoint>>();
		var group = app.MapGroup("/api");


		var endpointTypes = typeof(Program).Assembly
			.GetTypes()
			.Where(type => typeof(IEndpoint).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false });

		foreach(var endpointType in endpointTypes)
		{
			var inst = ActivatorUtilities.CreateInstance(app.Services, endpointType);

			if(inst is IEndpoint endpoint)
			{
				logger.LogInformation(
					"Mapping endpoint {EndpointType}",
					endpointType.Name);
				endpoint.MapEndpoint(group);
			}
		}
	}
}