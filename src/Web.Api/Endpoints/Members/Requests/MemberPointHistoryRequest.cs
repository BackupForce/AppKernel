namespace Web.Api.Endpoints.Members.Requests;

public sealed record MemberPointHistoryRequest(
    DateTime? StartDate,
    DateTime? EndDate,
    short? Type,
    string? ReferenceType,
    string? ReferenceId,
    int Page = 1,
    int PageSize = 20);
