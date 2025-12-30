using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using Domain.Users;
using SharedKernel;

namespace Application.Auth;

internal sealed class LoginOrRegisterByLineCommandHandler(
    IMemberExternalIdentityRepository memberExternalIdentityRepository,
    IMemberRepository memberRepository,
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    ITrackedUnitOfWork unitOfWork,
    IUniqueConstraintDetector uniqueConstraintDetector)
    : ICommandHandler<LoginOrRegisterByLineCommand, LineLoginResultDto>
{
    private const string Provider = "line";
    private const string ExternalIdentityUniqueConstraint = "ix_member_external_identities_provider_external_user_id";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<LineLoginResultDto>> Handle(
        LoginOrRegisterByLineCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.LineUserId))
        {
            return Result.Failure<LineLoginResultDto>(LineLoginErrors.LineUserIdRequired);
        }

        if (string.IsNullOrWhiteSpace(command.LineUserName))
        {
            return Result.Failure<LineLoginResultDto>(LineLoginErrors.LineUserNameRequired);
        }

        MemberExternalIdentity? externalIdentity = await memberExternalIdentityRepository
            .GetByProviderAndExternalUserIdAsync(Provider, command.LineUserId, cancellationToken);

        if (externalIdentity != null)
        {
            return await SignInExistingAsync(externalIdentity, command, false, cancellationToken);
        }

        return await RegisterAndSignInAsync(command, cancellationToken);
    }

    private async Task<Result<LineLoginResultDto>> RegisterAndSignInAsync(
        LoginOrRegisterByLineCommand command,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<Email> emailResult = Email.Create(GenerateLineEmail(command.LineUserId));
        if (emailResult.IsFailure)
        {
            return Result.Failure<LineLoginResultDto>(emailResult.Error);
        }

        if (!await userRepository.IsEmailUniqueAsync(emailResult.Value))
        {
            return Result.Failure<LineLoginResultDto>(UserErrors.EmailNotUnique);
        }

        string randomPassword = GenerateRandomPassword();
        var user = User.Create(emailResult.Value, new Name(command.LineUserName), passwordHasher.Hash(randomPassword), false);

        string memberNo = await GenerateMemberNoAsync(cancellationToken);

        Result<Member> memberResult = Member.Create(user.Id, memberNo, command.LineUserName, utcNow);
        if (memberResult.IsFailure)
        {
            return Result.Failure<LineLoginResultDto>(memberResult.Error);
        }

        Member member = memberResult.Value;
        MemberPointBalance pointBalance = MemberPointBalance.Create(member.Id, utcNow);

        Result<MemberExternalIdentity> identityResult = MemberExternalIdentity.Create(
            member.Id,
            Provider,
            command.LineUserId,
            command.LineUserName,
            utcNow);

        if (identityResult.IsFailure)
        {
            return Result.Failure<LineLoginResultDto>(identityResult.Error);
        }

        MemberExternalIdentity identity = identityResult.Value;

        using var transaction = await unitOfWork.BeginTransactionAsync();

        userRepository.Insert(user);
        memberRepository.Insert(member);
        memberRepository.UpsertPointBalance(pointBalance);
        await memberExternalIdentityRepository.AddAsync(identity, cancellationToken);

        MemberActivityLog activityLog = BuildActivityLog(member.Id, true, command.LineUserId, utcNow, user.Id);
        memberRepository.InsertActivity(activityLog);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            transaction.Commit();
        }
        catch (Exception ex) when (uniqueConstraintDetector.IsUniqueConstraint(ex, ExternalIdentityUniqueConstraint))
        {
            transaction.Rollback();
            unitOfWork.ClearChanges();

            MemberExternalIdentity? existingIdentity = await memberExternalIdentityRepository
                .GetByProviderAndExternalUserIdAsync(Provider, command.LineUserId, cancellationToken);

            if (existingIdentity == null)
            {
                throw;
            }

            return await SignInExistingAsync(existingIdentity, command, false, cancellationToken);
        }

        string token = GenerateToken(user);

        return Result.Success(new LineLoginResultDto
        {
            MemberId = member.Id,
            UserId = user.Id,
            AccessToken = token,
            IsNewMember = true,
            DisplayName = member.DisplayName,
            LineUserId = command.LineUserId
        });
    }

    private async Task<Result<LineLoginResultDto>> SignInExistingAsync(
        MemberExternalIdentity externalIdentity,
        LoginOrRegisterByLineCommand command,
        bool isNewMember,
        CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        Result updateResult = externalIdentity.UpdateExternalUserName(command.LineUserName, utcNow);
        if (updateResult.IsFailure)
        {
            return Result.Failure<LineLoginResultDto>(updateResult.Error);
        }

        Member? member = await memberRepository.GetByIdAsync(externalIdentity.MemberId, cancellationToken);
        if (member == null)
        {
            return Result.Failure<LineLoginResultDto>(MemberErrors.MemberNotFound);
        }

        if (!member.UserId.HasValue)
        {
            return Result.Failure<LineLoginResultDto>(LineLoginErrors.UserBindingMissing);
        }

        User? user = await userRepository.GetByIdAsync(member.UserId.Value, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LineLoginResultDto>(UserErrors.NotFound(member.UserId.Value));
        }

        await memberExternalIdentityRepository.UpdateAsync(externalIdentity, cancellationToken);

        MemberActivityLog activityLog = BuildActivityLog(
            member.Id,
            isNewMember,
            command.LineUserId,
            utcNow,
            member.UserId.Value);

        memberRepository.InsertActivity(activityLog);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        string token = GenerateToken(user);

        return Result.Success(new LineLoginResultDto
        {
            MemberId = member.Id,
            UserId = member.UserId.Value,
            AccessToken = token,
            IsNewMember = isNewMember,
            DisplayName = member.DisplayName,
            LineUserId = command.LineUserId
        });
    }

    private async Task<string> GenerateMemberNoAsync(CancellationToken cancellationToken)
    {
        string memberNo;
        do
        {
            memberNo = $"MBR-{dateTimeProvider.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
        }
        while (!await memberRepository.IsMemberNoUniqueAsync(memberNo, cancellationToken));

        return memberNo;
    }

    private static string GenerateLineEmail(string lineUserId)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(lineUserId));
        string hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return $"line_{hash}@line.local";
    }

    private static string GenerateRandomPassword()
    {
        Span<byte> buffer = stackalloc byte[16];
        RandomNumberGenerator.Fill(buffer);

        return Convert.ToHexString(buffer);
    }

    private static MemberActivityLog BuildActivityLog(
        Guid memberId,
        bool isNewMember,
        string lineUserId,
        DateTime utcNow,
        Guid operatorUserId)
    {
        string payload = JsonSerializer.Serialize(
            new
            {
                lineUserId,
                isNewMember
            },
            JsonOptions);

        return MemberActivityLog.Create(
            memberId,
            "auth.line_login",
            null,
            null,
            operatorUserId,
            payload,
            utcNow);
    }

    private string GenerateToken(User user)
    {
        return jwtService.GenerateToken(
            user.Id,
            user.Name.ToString(),
            user.Roles.Select(role => role.Name).ToArray(),
            Array.Empty<Guid>(),
            Array.Empty<string>());
    }
}
