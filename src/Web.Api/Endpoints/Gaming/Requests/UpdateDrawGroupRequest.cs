namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record UpdateDrawGroupRequest(
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc);
