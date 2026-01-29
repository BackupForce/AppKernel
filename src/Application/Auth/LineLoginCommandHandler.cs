using Application.Abstractions.Authentication;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Members;
using Domain.Users;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Auth;

internal sealed class LineLoginCommandHandler(
    ITenantContext tenantContext,
    ILineLoginPersistenceService lineLoginPersistenceService,
    IJwtService jwtService,
    ILineAuthService lineAuthService,
    ILogger<LineLoginCommandHandler> logger)
    : ICommandHandler<LineLoginCommand, LineLoginResponse>
{
    private const string DefaultMemberDisplayName = "LINE會員";

    public async Task<Result<LineLoginResponse>> Handle(
        LineLoginCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.AccessToken))
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineAccessTokenRequired);
        }

        if (!tenantContext.TryGetTenantId(out Guid tenantId))
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.TenantContextMissing);
        }

        // 中文註解：Application 只知道「拿 token 換 user id」，不關心 HTTP/JSON 細節。
        ExternalIdentityResult verifyResult = await lineAuthService.VerifyAccessTokenAsync(
            command.AccessToken,
            cancellationToken);

        if (!verifyResult.IsValid)
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineVerifyFailed);
        }

        if (string.IsNullOrWhiteSpace(verifyResult.LineUserId))
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineUserIdMissing);
        }

        string lineUserId = verifyResult.LineUserId;
        LineLoginPersistenceResult persistenceResult = await lineLoginPersistenceService.PersistAsync(
            tenantId,
            lineUserId,
            DefaultMemberDisplayName,
            null,
            null,
            null,
            command.UserAgent,
            command.Ip,
            command.DeviceId,
            cancellationToken);

        User user = persistenceResult.User;
        Member? member = persistenceResult.Member;

        (string Token, DateTime ExpiresAtUtc) accessToken = jwtService.IssueAccessToken(
            user.Id,
            user.Name.ToString(),
            user.Type,
            tenantId,
            Array.Empty<string>(),
            Array.Empty<Guid>(),
            Array.Empty<string>(),
            persistenceResult.IssuedAtUtc);

        LineLoginResponse response = new LineLoginResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = persistenceResult.RefreshToken,
            SessionId = persistenceResult.Session.Id,
            UserId = user.Id,
            TenantId = tenantId,
            MemberId = member?.Id,
            MemberNo = member?.MemberNo
        };

        logger.LogInformation("Line login succeeded for user {UserId}.", user.Id);

        return response;
    }
}
