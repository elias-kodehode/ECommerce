namespace ECommerce.Api.Tests.Common;

public sealed record ErrorResponse(IReadOnlyList<ErrorResponseItem> Errors);
