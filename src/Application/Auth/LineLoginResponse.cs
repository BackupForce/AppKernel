using System;

namespace Application.Auth;

public sealed class LineLoginResponse
{
    public string Token { get; init; } = string.Empty;

    public Guid UserId { get; init; }

    public Guid TenantId { get; init; }

    public Guid? MemberId { get; init; }

    public string? MemberNo { get; init; }
}
