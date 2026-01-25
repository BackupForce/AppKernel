namespace Web.Api.Endpoints.Admin.Requests;

public sealed record IssueMemberTicketsRequest(
    string GameCode,
    Guid DrawId,
    int Quantity,
    string? Reason,
    string? Note);
