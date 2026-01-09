using System;
using System.Text;
using Application.Abstractions.Authentication;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Extensions;
public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings settings)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(settings.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = securityKey,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // 中文註解：缺少 user_type/tenant_id 或解析失敗時直接拒絕，維持 Fail Closed。
                        if (!JwtUserContext.TryFromClaims(context.Principal, out _))
                        {
                            context.Fail("Invalid user_type or tenant_id.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
