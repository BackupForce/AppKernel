using SharedKernel;

namespace Domain.Members;

public sealed class MemberProfile : Entity
{
    private MemberProfile(
        Guid id,
        Guid memberId,
        string? realName,
        Gender gender,
        string? phoneNumber,
        bool phoneVerified,
        DateTime updatedAtUtc)
        : base(id)
    {
        MemberId = memberId;
        RealName = NormalizeOptional(realName);
        Gender = gender;
        PhoneNumber = NormalizeOptional(phoneNumber);
        PhoneVerified = phoneVerified;
        UpdatedAtUtc = updatedAtUtc;
    }

    private MemberProfile()
    {
    }

    public Guid MemberId { get; private set; }

    public string? RealName { get; private set; }

    public Gender Gender { get; private set; }

    public string? PhoneNumber { get; private set; }

    public bool PhoneVerified { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static MemberProfile Create(
        Guid memberId,
        string? realName,
        Gender gender,
        string? phoneNumber,
        bool phoneVerified,
        DateTime utcNow)
    {
        return new MemberProfile(
            Guid.NewGuid(),
            memberId,
            realName,
            gender,
            phoneNumber,
            phoneVerified,
            utcNow);
    }

    public void Update(
        string? realName,
        Gender gender,
        string? phoneNumber,
        bool phoneVerified,
        DateTime utcNow)
    {
        RealName = NormalizeOptional(realName);
        Gender = gender;
        PhoneNumber = NormalizeOptional(phoneNumber);
        PhoneVerified = phoneVerified;
        UpdatedAtUtc = utcNow;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
