namespace Web.Api.Endpoints.Admin.MemberTags.Requests;

public sealed record ListMemberTagsRequest(
    string? Keyword,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);
