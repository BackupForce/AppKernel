using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Domain.Users;

namespace Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly int _expireMinutes;

    public JwtService(string secretKey, int expireMinutes = 60)
    {
        _secretKey = secretKey;
        _expireMinutes = expireMinutes;
    }

    public string GenerateToken(
         Guid userId,
         string userName,
         UserType userType,
         Guid? tenantId,
         IEnumerable<string> roles,
         IEnumerable<Guid> nodeIds,
         IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(JwtClaimNames.UserType, userType.ToString()),
            new Claim(ClaimTypes.Role, string.Join(",", roles)),
            new Claim("nodes", string.Join(",", nodeIds)),
            new Claim("permissions", string.Join(",", permissions))
        };

        if (tenantId.HasValue)
        {
            // 中文註解：Platform 不發 tenant_id，其餘類型必須帶出租戶識別碼。
            claims.Add(new Claim(JwtClaimNames.TenantId, tenantId.Value.ToString("D")));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "appkernel",
            audience: "appkernel",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtPayloadDto? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] keyBytes = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "appkernel",
                ValidAudience = "appkernel",
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero
            }, out _);

            string? userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? userName = principal.FindFirst(ClaimTypes.Name)?.Value;
            string? rolesStr = principal.FindFirst(ClaimTypes.Role)?.Value;
            string? userTypeStr = principal.FindFirst(JwtClaimNames.UserType)?.Value;
            string? tenantIdStr = principal.FindFirst(JwtClaimNames.TenantId)?.Value;
            string? nodesStr = principal.FindFirst("nodes")?.Value;
            string? permissionsStr = principal.FindFirst("permissions")?.Value;

            Guid userId = Guid.TryParse(userIdStr, out Guid parsedUserId) ? parsedUserId : Guid.Empty;
            if (!UserTypeParser.TryParse(userTypeStr, out UserType parsedUserType))
            {
                return null;
            }

            Guid? tenantId = null;
            if (parsedUserType != UserType.Platform)
            {
                if (!Guid.TryParse(tenantIdStr, out Guid parsedTenantId) || parsedTenantId == Guid.Empty)
                {
                    return null;
                }

                tenantId = parsedTenantId;
            }
            else if (!string.IsNullOrWhiteSpace(tenantIdStr))
            {
                return null;
            }

            return new JwtPayloadDto
            {
                UserId = userId,
                UserName = userName ?? string.Empty,
                UserType = parsedUserType,
                TenantId = tenantId,
                Roles = (rolesStr ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                NodeIds = (nodesStr ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Guid.TryParse(id, out Guid g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList(),
                Permissions = (permissionsStr ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            };
        }
        catch
        {
            return null;
        }
    }

}
