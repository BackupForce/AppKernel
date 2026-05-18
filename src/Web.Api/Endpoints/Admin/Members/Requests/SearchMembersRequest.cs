namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record SearchMembersRequest(
    string? Keyword,
    string? MemberNo,
    string? DisplayName,
    string? PhoneNumber,
    short? Status,
    Guid? UserId,
    int Page = 1,
    int PageSize = 20);
