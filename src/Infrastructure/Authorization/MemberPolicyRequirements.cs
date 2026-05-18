using Application.Abstractions.Authentication;
using Domain.Members;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

internal sealed class ActiveMemberRequirement : IAuthorizationRequirement
{
}

internal sealed class MemberOwnerRequirement : IAuthorizationRequirement
{
}

internal sealed class ActiveMemberAuthorizationHandler
    : AuthorizationHandler<ActiveMemberRequirement>
{
    private readonly ApplicationDbContext _dbContext;

    public ActiveMemberAuthorizationHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveMemberRequirement requirement)
    {
        if (!JwtUserContext.TryFromClaims(context.User, out JwtUserContext? jwtContext) || jwtContext is null)
        {
            return;
        }

        if (jwtContext.UserType != UserType.Member || !jwtContext.TenantId.HasValue)
        {
            return;
        }

        Member? member = await _dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == jwtContext.UserId && m.TenantId == jwtContext.TenantId.Value);

        if (member is null)
        {
            return;
        }

        if (member.Status != MemberStatus.Active)
        {
            // 中文註解：停用或刪除會員一律拒絕。
            return;
        }

        context.Succeed(requirement);
    }
}

internal sealed class MemberOwnerAuthorizationHandler
    : AuthorizationHandler<MemberOwnerRequirement>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MemberOwnerAuthorizationHandler(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MemberOwnerRequirement requirement)
    {
        if (!JwtUserContext.TryFromClaims(context.User, out JwtUserContext? jwtContext) || jwtContext is null)
        {
            return;
        }

        if (jwtContext.UserType != UserType.Member || !jwtContext.TenantId.HasValue)
        {
            return;
        }

        Guid? memberId = ResolveMemberId(context);
        if (!memberId.HasValue)
        {
            // 中文註解：缺少 memberId 代表無法驗證擁有者，直接拒絕。
            return;
        }

        Member? member = await _dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == memberId.Value && m.TenantId == jwtContext.TenantId.Value);

        if (member is null || !member.UserId.HasValue)
        {
            return;
        }

        if (member.UserId.Value != jwtContext.UserId)
        {
            // 中文註解：會員只能操作自己的資源。
            return;
        }

        context.Succeed(requirement);
    }

    private Guid? ResolveMemberId(AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.RouteValues.TryGetValue("memberId", out object? memberValue)
            && TryGetGuid(memberValue, out Guid memberId))
        {
            return memberId;
        }

        if (httpContext.Request.RouteValues.TryGetValue("id", out object? idValue)
            && TryGetGuid(idValue, out Guid fallbackId))
        {
            return fallbackId;
        }

        return null;
    }

    private static bool TryGetGuid(object? value, out Guid id)
    {
        if (value is Guid guidValue)
        {
            id = guidValue;
            return true;
        }

        if (value is string stringValue && Guid.TryParse(stringValue, out Guid parsedId))
        {
            id = parsedId;
            return true;
        }

        id = Guid.Empty;
        return false;
    }
}
