namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record CreateCampaignRequest(
    string GameCode,
    string PlayTypeCode,
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc);
