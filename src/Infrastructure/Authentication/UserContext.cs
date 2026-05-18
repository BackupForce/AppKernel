using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        ResolveContext().UserId;

    public UserType UserType =>
        ResolveContext().UserType;

    public Guid? TenantId =>
        ResolveContext().TenantId;

    private JwtUserContext ResolveContext()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext ?? throw new ApplicationException("User context is unavailable");

        if (!JwtUserContext.TryFromClaims(httpContext.User, out JwtUserContext? context) || context is null)
        {
            throw new ApplicationException("User context is unavailable");
        }

        return context;
    }
}
