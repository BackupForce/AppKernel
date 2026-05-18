namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record ReplaceMemberTagsRequest(
    IReadOnlyCollection<Guid> TagIds);
