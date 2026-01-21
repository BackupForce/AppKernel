using Application.Abstractions.Authentication;
using Application.Abstractions.Identity;
using Domain.Auth;
using Domain.Members;
using Domain.Security;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Authentication;

internal sealed class LineLoginPersistenceService(
    ApplicationDbContext dbContext,
    IUserRepository userRepository,
    IUserLoginBindingReader userLoginBindingReader,
    IMemberRepository memberRepository,
    IResourceNodeRepository resourceNodeRepository,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher,
    IMemberNoGenerator memberNoGenerator,
    ILineLoginSettings lineLoginSettings,
    IAuthTokenSettings authTokenSettings,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    ILogger<LineLoginPersistenceService> logger)
    : ILineLoginPersistenceService
{
    public async Task<LineLoginPersistenceResult> PersistAsync(
        Guid tenantId,
        string lineUserId,
        string displayName,
        string? userAgent,
        string? ip,
        string? deviceId,
        CancellationToken cancellationToken)
    {
        string normalizedLineUserId = LoginBinding.Normalize(LoginProvider.Line, lineUserId);

        User? user = await userLoginBindingReader.FindUserByLoginAsync(
            tenantId,
            LoginProvider.Line,
            normalizedLineUserId,
            cancellationToken);
        Member? member = null;
        bool isNewMember = false;

        if (user is null)
        {
            MemberLoginCreation creation = await CreateMemberUserAsync(
                tenantId,
                lineUserId,
                displayName,
                cancellationToken);
            user = creation.User;
            member = creation.Member;
            isNewMember = true;
        }

        if (member is null)
        {
            member = await memberRepository.GetByUserIdAsync(tenantId, user.Id, cancellationToken);
            if (member is null)
            {
                member = await CreateMemberForExistingUserAsync(tenantId, user.Id, displayName, cancellationToken);
                isNewMember = true;
            }
        }

        LineLoginPersistenceResult result = await CreateSessionAsync(
            tenantId,
            user,
            member,
            userAgent,
            ip,
            deviceId,
            lineUserId,
            cancellationToken);

        return result with { IsNewMember = isNewMember };
    }

    private async Task<LineLoginPersistenceResult> CreateSessionAsync(
        Guid tenantId,
        User user,
        Member? member,
        string? userAgent,
        string? ip,
        string? deviceId,
        string lineUserId,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        DateTime sessionExpiresAtUtc = utcNow.AddDays(authTokenSettings.RefreshTokenTtlDays);

        AuthSession session = AuthSession.Create(
            tenantId,
            user.Id,
            utcNow,
            sessionExpiresAtUtc,
            userAgent,
            ip,
            deviceId);
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

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException ex)
        {
            logger.LogWarning(ex, "中文註解：LINE 登入併發新增失敗，改用重查流程。");
            dbContext.ChangeTracker.Clear();

            User? existingUser = await userLoginBindingReader.FindUserByLoginAsync(
                tenantId,
                LoginProvider.Line,
                LoginBinding.Normalize(LoginProvider.Line, lineUserId),
                cancellationToken);
            if (existingUser is null)
            {
                throw;
            }

            Member? existingMember = await memberRepository.GetByUserIdAsync(tenantId, existingUser.Id, cancellationToken);
            existingMember ??= await CreateMemberForExistingUserAsync(
                    tenantId,
                    existingUser.Id,
                    existingUser.Name.ToString(),
                    cancellationToken);

            return await CreateSessionAsync(
                tenantId,
                existingUser,
                existingMember,
                userAgent,
                ip,
                deviceId,
                lineUserId,
                cancellationToken);
        }

        return new LineLoginPersistenceResult(user, member, session, refreshTokenPlain, utcNow);
    }

    private async Task<MemberLoginCreation> CreateMemberUserAsync(
        Guid tenantId,
        string lineUserId,
        string displayName,
        CancellationToken cancellationToken)
    {
        string emailValue = $"{lineUserId.Trim()}@{lineLoginSettings.EmailDomain}";
        Result<Email> emailResult = Email.Create(emailValue);
        if (emailResult.IsFailure)
        {
            throw new InvalidOperationException("Line login email generation failed.");
        }

        Name name = new Name(displayName);
        string passwordHash = passwordHasher.Hash(Guid.NewGuid().ToString("N"));

        User user = User.Create(
            emailResult.Value,
            name,
            passwordHash,
            false,
            UserType.Member,
            tenantId);

        Result bindResult = user.BindLogin(LoginProvider.Line, lineUserId, dateTimeProvider.UtcNow);
        if (bindResult.IsFailure)
        {
            throw new InvalidOperationException(bindResult.Error.Description);
        }

        Member member = await CreateMemberEntityAsync(tenantId, user.Id, displayName, cancellationToken);

        userRepository.Insert(user);
        memberRepository.Insert(member);

        DateTime utcNow = dateTimeProvider.UtcNow;
        MemberPointBalance pointBalance = MemberPointBalance.Create(member.Id, utcNow);
        memberRepository.InsertPointBalance(pointBalance);

        Guid? parentNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        ResourceNode memberNode = ResourceNode.Create(
            member.DisplayName,
            ResourceNodeKeys.Member(member.Id),
            tenantId,
            parentNodeId);
        resourceNodeRepository.Insert(memberNode);

        return new MemberLoginCreation(user, member);
    }

    private async Task<Member> CreateMemberForExistingUserAsync(
        Guid tenantId,
        Guid userId,
        string displayName,
        CancellationToken cancellationToken)
    {
        Member member = await CreateMemberEntityAsync(tenantId, userId, displayName, cancellationToken);
        memberRepository.Insert(member);

        DateTime utcNow = dateTimeProvider.UtcNow;
        MemberPointBalance pointBalance = MemberPointBalance.Create(member.Id, utcNow);
        memberRepository.InsertPointBalance(pointBalance);

        Guid? parentNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        ResourceNode memberNode = ResourceNode.Create(
            member.DisplayName,
            ResourceNodeKeys.Member(member.Id),
            tenantId,
            parentNodeId);
        resourceNodeRepository.Insert(memberNode);

        return member;
    }

    private async Task<Member> CreateMemberEntityAsync(
        Guid tenantId,
        Guid userId,
        string displayName,
        CancellationToken cancellationToken)
    {
        string memberNo = await memberNoGenerator.GenerateAsync(
            tenantId,
            MemberNoGenerationMode.TenantPrefix,
            cancellationToken);
        DateTime utcNow = dateTimeProvider.UtcNow;
        Result<Member> memberResult = Member.Create(tenantId, userId, memberNo, displayName, utcNow);
        if (memberResult.IsFailure)
        {
            throw new InvalidOperationException(memberResult.Error.Description);
        }

        return memberResult.Value;
    }

    private sealed record MemberLoginCreation(User User, Member Member);
}
