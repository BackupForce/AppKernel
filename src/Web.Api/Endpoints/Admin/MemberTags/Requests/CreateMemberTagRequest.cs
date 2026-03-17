namespace Web.Api.Endpoints.Admin.MemberTags.Requests;

public sealed record CreateMemberTagRequest(
    string TagCode,
    string DisplayName);
