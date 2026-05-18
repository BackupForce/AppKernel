using System.Security.Claims;
using Domain.Users;

namespace Application.Abstractions.Authentication;

public sealed class JwtUserContext
{
    public JwtUserContext(Guid userId, UserType userType, Guid? tenantId)
    {
        UserId = userId;
        UserType = userType;
        TenantId = tenantId;
    }

    public Guid UserId { get; }

    public UserType UserType { get; }

    public Guid? TenantId { get; }

    public static bool TryFromClaims(ClaimsPrincipal? principal, out JwtUserContext? context)
    {
        context = null;

        if (principal is null)
        {
            return false;
        }

        string? userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out Guid userId) || userId == Guid.Empty)
        {
            return false;
        }

        string? userTypeValue = principal.FindFirst(JwtClaimNames.UserType)?.Value;
        if (!UserTypeParser.TryParse(userTypeValue, out UserType userType))
        {
            return false;
        }

        string? tenantValue = principal.FindFirst(JwtClaimNames.TenantId)?.Value;
        Guid? tenantId = null;

        if (userType == UserType.Platform)
        {
            // 中文註解：平台帳號禁止攜帶 tenant_id，避免錯誤分流。
            if (!string.IsNullOrWhiteSpace(tenantValue))
            {
                return false;
            }
        }
        else
        {
            if (!Guid.TryParse(tenantValue, out Guid parsedTenantId) || parsedTenantId == Guid.Empty)
            {
                return false;
            }

            tenantId = parsedTenantId;
        }

        context = new JwtUserContext(userId, userType, tenantId);
        return true;
    }
}
