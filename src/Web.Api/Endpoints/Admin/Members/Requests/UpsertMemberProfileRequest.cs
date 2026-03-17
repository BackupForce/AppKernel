namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record UpsertMemberProfileRequest(
    string? RealName,
    short Gender,
    string? PhoneNumber,
    bool PhoneVerified);
