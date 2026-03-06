namespace Web.Api.Endpoints.Admin.Requests;

public sealed record CreateMemberTagRequest(
    string TagCode,
    string DisplayName);

public sealed record UpdateMemberTagRequest(
    string DisplayName);

public sealed record ReplaceMemberTagsRequest(
    IReadOnlyCollection<Guid> TagIds);

public sealed record ListMemberTagsRequest(
    string? Keyword,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);
