namespace Web.Api.Endpoints.Members.Requests;

public sealed record SearchMembersRequest(
    string? MemberNo,
    string? DisplayName,
    short? Status,
    Guid? UserId,
    int Page = 1,
    int PageSize = 20);
