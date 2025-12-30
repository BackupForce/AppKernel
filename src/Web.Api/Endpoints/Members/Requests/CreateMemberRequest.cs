namespace Web.Api.Endpoints.Members.Requests;

public sealed record CreateMemberRequest(Guid? UserId, string DisplayName, string? MemberNo);
