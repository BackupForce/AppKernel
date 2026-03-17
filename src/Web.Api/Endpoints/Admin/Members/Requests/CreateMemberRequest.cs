namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record CreateMemberRequest(Guid? UserId, string DisplayName, string? MemberNo);
