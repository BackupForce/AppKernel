using SharedKernel;

namespace Domain.Members;

public sealed class MemberExternalIdentity : Entity
{
    private MemberExternalIdentity(
        Guid id,
        Guid memberId,
        string provider,
        string externalUserId,
        string externalUserName,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        MemberId = memberId;
        Provider = provider;
        ExternalUserId = externalUserId;
        ExternalUserName = externalUserName;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private MemberExternalIdentity()
    {
    }

    public Guid MemberId { get; private set; }

    public string Provider { get; private set; } = string.Empty;

    public string ExternalUserId { get; private set; } = string.Empty;

    public string ExternalUserName { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<MemberExternalIdentity> Create(
        Guid memberId,
        string provider,
        string externalUserId,
        string externalUserName,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Result.Failure<MemberExternalIdentity>(MemberExternalIdentityErrors.ProviderRequired);
        }

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return Result.Failure<MemberExternalIdentity>(MemberExternalIdentityErrors.ExternalUserIdRequired);
        }

        if (string.IsNullOrWhiteSpace(externalUserName))
        {
            return Result.Failure<MemberExternalIdentity>(MemberExternalIdentityErrors.ExternalUserNameRequired);
        }

        var identity = new MemberExternalIdentity(
            Guid.NewGuid(),
            memberId,
            provider,
            externalUserId,
            externalUserName,
            utcNow,
            utcNow);

        return identity;
    }

    public Result UpdateExternalUserName(string externalUserName, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(externalUserName))
        {
            return Result.Failure(MemberExternalIdentityErrors.ExternalUserNameRequired);
        }

        if (ExternalUserName == externalUserName)
        {
            return Result.Success();
        }

        ExternalUserName = externalUserName;
        UpdatedAt = utcNow;

        return Result.Success();
    }
}
