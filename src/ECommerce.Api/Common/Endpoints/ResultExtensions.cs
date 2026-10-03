using ECommerce.Api.Common.Results;
using static Microsoft.AspNetCore.Http.Results;
namespace ECommerce.Api.Common.Endpoints;

public static class ResultExtensions
{
	public static IResult ToProblemDetails(this IReadOnlyList<Error> errors)
	{
		ArgumentNullException.ThrowIfNull(errors);
		if(errors.Count == 0)
		{
			throw new ArgumentException("At least one error is required.", nameof(errors));
		}
		var status = errors[0].Type switch
		{
			ErrorType.Validation => StatusCodes.Status400BadRequest,
			ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
			ErrorType.Forbidden => StatusCodes.Status403Forbidden,
			ErrorType.NotFound => StatusCodes.Status404NotFound,
			ErrorType.Conflict => StatusCodes.Status409Conflict,
			_ => StatusCodes.Status500InternalServerError
		};

		var title = errors[0].Type switch
		{
			ErrorType.Validation => "Validation failed.",
			ErrorType.Unauthorized => "Authentication is required.",
			ErrorType.Forbidden => "Access is forbidden.",
			ErrorType.NotFound => "The requested resource was not found.",
			ErrorType.Conflict => "A conflict occurred.",
			_ => "An unexpected error occurred."
		};


		if(status == StatusCodes.Status500InternalServerError)
		{
			return Problem(statusCode: status, title: title);
		}
		return Problem(
			statusCode: status,
			title: title,
			extensions: new Dictionary<string, object?>
			{
				["errors"] = errors.Select(e => new
				{
					e.Code,
					e.Description
				}).ToArray()
			});
	}
}
