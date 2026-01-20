using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Auth;
using Domain.Members;
using Domain.Security;
using Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Security.Cryptography; // 新增引用

namespace Application.Auth;

internal sealed class LineLoginCommandHandler(
    IUserRepository userRepository,
    IMemberRepository memberRepository,
    IResourceNodeRepository resourceNodeRepository,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IExternalIdentityVerifier externalIdentityVerifier,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IOptions<AuthTokenOptions> authTokenOptions,
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

        Guid tenantId;
        try
        {
            tenantId = tenantContext.TenantId;
        }
        catch
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.TenantContextMissing);
        }

        // 中文註解：Application 只知道「拿 token 換 user id」，不關心 HTTP/JSON 細節。
        ExternalIdentityResult verifyResult = await externalIdentityVerifier.VerifyLineAccessTokenAsync(
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
        string normalizedLineUserId = NormalizeForLookup(lineUserId);

        User? user = await userRepository.GetMemberByNormalizedLineUserIdAsync(
            tenantId,
            normalizedLineUserId,
            cancellationToken);
        Member? member = null;

        if (user is null)
        {
            // 中文註解：首次登入建立使用者/會員資料，若遇到併發重試則重新查詢。
            try
            {
                MemberLoginCreation creation = await CreateMemberUserAsync(
                    tenantId,
                    lineUserId,
                    cancellationToken);
                user = creation.User;
                member = creation.Member;
            }
            catch (UniqueConstraintViolationException ex)
            {
                logger.LogWarning(ex, "中文註解：LINE 登入併發新增失敗，改走重查流程。");
                user = await userRepository.GetMemberByNormalizedLineUserIdAsync(
                    tenantId,
                    normalizedLineUserId,
                    cancellationToken);
                if (user is null)
                {
                    return Result.Failure<LineLoginResponse>(AuthErrors.LineVerifyFailed);
                }
            }
        }

        if (member is null && user is not null)
        {
            member = await memberRepository.GetByUserIdAsync(tenantId, user.Id, cancellationToken);
            if (member is null)
            {
                // 中文註解：若會員資料缺失，補齊 Member 與資源節點，保持流程可重入。
                try
                {
                    member = await CreateMemberForExistingUserAsync(tenantId, user.Id, cancellationToken);
                }
                catch (UniqueConstraintViolationException ex)
                {
                    logger.LogWarning(ex, "中文註解：補齊會員資料時發生競態，改用重查結果。");
                    member = await memberRepository.GetByUserIdAsync(tenantId, user.Id, cancellationToken);
                }
            }
        }

        if (user is null)
        {
            return Result.Failure<LineLoginResponse>(AuthErrors.LineVerifyFailed);
        }

        AuthTokenOptions options = authTokenOptions.Value;
        DateTime utcNow = dateTimeProvider.UtcNow;
        DateTime sessionExpiresAtUtc = utcNow.AddDays(options.RefreshTokenTtlDays);

        AuthSession session = AuthSession.Create(
            tenantId,
            user.Id,
            utcNow,
            sessionExpiresAtUtc,
            command.UserAgent,
            command.Ip,
            command.DeviceId);
        session.Touch(utcNow);
        authSessionRepository.Insert(session);

        string refreshTokenPlain = refreshTokenGenerator.GenerateToken();
        string refreshTokenHash = refreshTokenHasher.Hash(refreshTokenPlain);

        RefreshTokenRecord refreshTokenRecord = RefreshTokenRecord.Create(
            session.Id,
            refreshTokenHash,
            utcNow,
            sessionExpiresAtUtc);
        refreshTokenRepository.Insert(refreshTokenRecord);

        (string Token, DateTime ExpiresAtUtc) accessToken = jwtService.IssueAccessToken(
            user.Id,
            user.Name.ToString(),
            user.Type,
            tenantId,
            Array.Empty<string>(),
            Array.Empty<Guid>(),
            Array.Empty<string>(),
            utcNow);

        LineLoginResponse response = new LineLoginResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshTokenPlain,
            SessionId = session.Id,
            UserId = user.Id,
            TenantId = tenantId,
            MemberId = member?.Id,
            MemberNo = member?.MemberNo
        };

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    private async Task<MemberLoginCreation> CreateMemberUserAsync(
        Guid tenantId,
        string lineUserId,
        CancellationToken cancellationToken)
    {
        string emailValue = $"{lineUserId.Trim()}@line.local";
        Result<Email> emailResult = Email.Create(emailValue);
        if (emailResult.IsFailure)
        {
            throw new InvalidOperationException("Line login email generation failed.");
        }

        Name name = new Name(DefaultMemberDisplayName);
        string passwordHash = passwordHasher.Hash(Guid.NewGuid().ToString("N"));

        User user = User.Create(
            emailResult.Value,
            name,
            passwordHash,
            false,
            UserType.Member,
            tenantId,
            lineUserId);

        Member member = await CreateMemberEntityAsync(tenantId, user.Id, cancellationToken);

        userRepository.Insert(user);
        memberRepository.Insert(member);

        DateTime utcNow = dateTimeProvider.UtcNow;
        MemberPointBalance pointBalance = MemberPointBalance.Create(member.Id, utcNow);
        memberRepository.InsertPointBalance(pointBalance);

        Guid? parentNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        ResourceNode memberNode = ResourceNode.Create(
            member.DisplayName,
            $"member:{member.Id:D}",
            tenantId,
            parentNodeId);
        resourceNodeRepository.Insert(memberNode);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new MemberLoginCreation(user, member);
    }

    private async Task<Member> CreateMemberForExistingUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        Member member = await CreateMemberEntityAsync(tenantId, userId, cancellationToken);
        memberRepository.Insert(member);

        DateTime utcNow = dateTimeProvider.UtcNow;
        MemberPointBalance pointBalance = MemberPointBalance.Create(member.Id, utcNow);
        memberRepository.InsertPointBalance(pointBalance);

        Guid? parentNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        ResourceNode memberNode = ResourceNode.Create(
            member.DisplayName,
            $"member:{member.Id:D}",
            tenantId,
            parentNodeId);
        resourceNodeRepository.Insert(memberNode);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return member;
    }

    private async Task<Member> CreateMemberEntityAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        string memberNo = await GenerateMemberNoAsync(tenantId, cancellationToken);
        DateTime utcNow = dateTimeProvider.UtcNow;
        Result<Member> memberResult = Member.Create(tenantId, userId, memberNo, DefaultMemberDisplayName, utcNow);
        if (memberResult.IsFailure)
        {
            throw new InvalidOperationException(memberResult.Error.Description);
        }

        return memberResult.Value;
    }

    private async Task<string> GenerateMemberNoAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        string prefix = tenantId.ToString("N")[..6].ToUpperInvariant();

        string candidate;
        do
        {
            // 使用密碼編譯安全性亂數產生器
            int randomNumber;
            byte[] bytes = new byte[2];
            RandomNumberGenerator.Fill(bytes);
            randomNumber = BitConverter.ToUInt16(bytes, 0) % 10000;
            string randomDigits = randomNumber.ToString("0000", System.Globalization.CultureInfo.InvariantCulture);
            candidate = $"{prefix}{randomDigits}";
        } while (!await memberRepository.IsMemberNoUniqueAsync(tenantId, candidate, cancellationToken));

        return candidate;
    }

    private static string NormalizeForLookup(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private sealed record MemberLoginCreation(User User, Member Member);
}
