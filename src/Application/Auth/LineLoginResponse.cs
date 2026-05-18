using System;

namespace Application.Auth;

public sealed class LineLoginResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public string? RefreshToken { get; init; }

    public Guid SessionId { get; init; }

    public Guid UserId { get; init; }

    public Guid TenantId { get; init; }

    public Guid? MemberId { get; init; }

    public string? MemberNo { get; init; }
}
