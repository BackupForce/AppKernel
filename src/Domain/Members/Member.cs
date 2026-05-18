using Domain.Members.Events;
using SharedKernel;

namespace Domain.Members;

public sealed class Member : Entity
{
    private Member(
        Guid id,
        Guid tenantId,
        Guid? userId,
        string memberNo,
        string displayName,
        MemberStatus status,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        MemberNo = memberNo;
        DisplayName = displayName;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private Member()
    {
    }

    public Guid? UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public string MemberNo { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public MemberStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Member> Create(
        Guid tenantId,
        Guid? userId,
        string memberNo,
        string displayName,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Member>(MemberErrors.TenantIdRequired);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<Member>(MemberErrors.DisplayNameRequired);
        }

        if (string.IsNullOrWhiteSpace(memberNo))
        {
            return Result.Failure<Member>(MemberErrors.MemberNoRequired);
        }

        var member = new Member(
            Guid.NewGuid(),
            tenantId,
            userId,
            memberNo,
            displayName,
            MemberStatus.Active,
            utcNow,
            utcNow);

        member.Raise(new MemberCreatedDomainEvent(member.Id, member.MemberNo));

        return member;
    }

    public Result UpdateProfile(string displayName, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(MemberErrors.DisplayNameRequired);
        }

        DisplayName = displayName;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    public Result Suspend(DateTime utcNow)
    {
        if (Status == MemberStatus.Deleted)
        {
            return Result.Failure(MemberErrors.InvalidStatusTransition);
        }

        if (Status == MemberStatus.Suspended)
        {
            return Result.Success();
        }

        Status = MemberStatus.Suspended;
        UpdatedAt = utcNow;

        Raise(new MemberStatusChangedDomainEvent(Id, Status));

        return Result.Success();
    }

    public Result Activate(DateTime utcNow)
    {
        if (Status == MemberStatus.Deleted)
        {
            return Result.Failure(MemberErrors.InvalidStatusTransition);
        }

        if (Status == MemberStatus.Active)
        {
            return Result.Success();
        }

        Status = MemberStatus.Active;
        UpdatedAt = utcNow;

        Raise(new MemberStatusChangedDomainEvent(Id, Status));

        return Result.Success();
    }

    public void RegisterPointsAdjusted(
        MemberPointLedgerType type,
        long amount,
        long beforeBalance,
        long afterBalance,
        Guid? operatorUserId)
    {
        Raise(new MemberPointsAdjustedDomainEvent(Id, type, amount, beforeBalance, afterBalance, operatorUserId));
    }

    public void RegisterAssetAdjusted(
        string assetCode,
        MemberAssetLedgerType type,
        decimal amount,
        decimal beforeBalance,
        decimal afterBalance,
        Guid? operatorUserId)
    {
        Raise(new MemberAssetAdjustedDomainEvent(Id, assetCode, type, amount, beforeBalance, afterBalance, operatorUserId));
    }
}
