using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Members;
using Domain.Users;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Auth;

internal sealed class LineLiffLoginCommandHandler(
    ITenantContext tenantContext,
    IUserLoginBindingReader loginBindingReader,
    ILineLoginPersistenceService lineLoginPersistenceService,
    IJwtService jwtService,
    ILineAuthService lineAuthService,
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<LineLiffLoginCommandHandler> logger)
    : ICommandHandler<LineLiffLoginCommand, LineLoginResponse>
{
    private const string DefaultMemberDisplayName = "LINE會員";
    private const string MemberAutoRegisterAction = "member.auto_register.liff";

    public async Task<Result<LineLoginResponse>> Handle(
        LineLiffLoginCommand command,
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
        string normalizedKey = LoginBinding.Normalize(LoginProvider.Line, lineUserId);

        User? existingUser = await loginBindingReader.FindUserByLoginAsync(
            tenantId,
            LoginProvider.Line,
            normalizedKey,
            cancellationToken);
        if (existingUser is not null && !existingUser.IsMember())
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineLoginUserTypeInvalid);
        }

        string displayName = string.IsNullOrWhiteSpace(verifyResult.DisplayName)
            ? DefaultMemberDisplayName
            : verifyResult.DisplayName.Trim();
        string? profileDisplayName = string.IsNullOrWhiteSpace(verifyResult.DisplayName)
            ? null
            : verifyResult.DisplayName.Trim();

        Uri? pictureUrl = null;
        if (!string.IsNullOrWhiteSpace(verifyResult.PictureUrl)
            && Uri.TryCreate(verifyResult.PictureUrl, UriKind.Absolute, out Uri? parsedPictureUrl))
        {
            pictureUrl = parsedPictureUrl;
        }

        LineLoginPersistenceResult persistenceResult = await lineLoginPersistenceService.PersistAsync(
            tenantId,
            lineUserId,
            displayName,
            profileDisplayName,
            pictureUrl,
            command.Email,
            command.UserAgent,
            command.Ip,
            command.DeviceId,
            cancellationToken);

        User user = persistenceResult.User;
        Member? member = persistenceResult.Member;

        if (!user.IsMember())
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineLoginUserTypeInvalid);
        }

        if (persistenceResult.IsNewMember && member is not null)
        {
            string? payload = "{\"source\":\"liff\",\"provider\":\"line\"}";
            MemberActivityLog log = MemberActivityLog.Create(
                member.Id,
                MemberAutoRegisterAction,
                command.Ip,
                command.UserAgent,
                null,
                payload,
                dateTimeProvider.UtcNow);
            memberRepository.InsertActivity(log);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

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

        logger.LogInformation("LINE LIFF login succeeded for user {UserId}.", user.Id);

        return response;
    }
}
