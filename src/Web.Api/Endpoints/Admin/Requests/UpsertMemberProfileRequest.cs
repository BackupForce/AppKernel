namespace Web.Api.Endpoints.Admin.Requests;

public sealed record UpsertMemberProfileRequest(
    string? RealName,
    short Gender,
    string? PhoneNumber,
    bool PhoneVerified);
