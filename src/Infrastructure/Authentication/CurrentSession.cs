using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class CurrentSession(IHttpContextAccessor httpContextAccessor) : ICurrentSession
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid? TenantId =>
        TryGetGuidClaim(JwtClaimNames.TenantId);

    public Guid? AuthSessionId =>
        TryGetGuidClaim(JwtClaimNames.AuthSessionId);

    private Guid? TryGetGuidClaim(string claimType)
    {
        string? claimValue = httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;

        return Guid.TryParse(claimValue, out Guid parsed) && parsed != Guid.Empty
            ? parsed
            : null;
    }
}
