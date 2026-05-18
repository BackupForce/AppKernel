namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;

public sealed record CreateDrawGroupRequest(
    string GameCode,
    string PlayTypeCode,
    string Name,
    DateTime? GrantOpenAtUtc,
    DateTime? GrantCloseAtUtc);
