namespace Application.Auth;

public sealed class LineLoginResultDto
{
    public Guid MemberId { get; init; }

    public Guid UserId { get; init; }

    public string AccessToken { get; init; } = string.Empty;

    public bool IsNewMember { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string LineUserId { get; init; } = string.Empty;
}
