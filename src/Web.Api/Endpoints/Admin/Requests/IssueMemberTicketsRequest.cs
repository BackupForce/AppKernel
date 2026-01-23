namespace Web.Api.Endpoints.Admin.Requests;

public sealed record IssueMemberTicketsRequest(
    string GameCode,
    string PlayTypeCode,
    Guid DrawId,
    int Quantity,
    string? Reason,
    string? Note);
