namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record UpdateCampaignRequest(
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc);
