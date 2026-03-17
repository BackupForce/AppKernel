namespace Web.Api.Endpoints.Admin.Me.Responses;

public sealed record GetMyPermissionsResponse(IReadOnlyList<string> Items);
