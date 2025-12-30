namespace Web.Api.Endpoints.Members.Requests;

public sealed record MemberActivityRequest(
    DateTime? StartDate,
    DateTime? EndDate,
    string? Action,
    int Page = 1,
    int PageSize = 20);
