using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.TicketClaimEvents;

public sealed class TicketClaimEvent : Entity
{
    private TicketClaimEvent(
        Guid id,
        Guid tenantId,
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        TicketClaimEventStatus status,
        int totalQuota,
        int totalClaimed,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = status;
        TotalQuota = totalQuota;
        TotalClaimed = totalClaimed;
        PerMemberQuota = perMemberQuota;
        ScopeType = scopeType;
        ScopeId = scopeId;
        TicketTemplateId = ticketTemplateId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    private TicketClaimEvent()
    {
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public TicketClaimEventStatus Status { get; private set; }

    public int TotalQuota { get; private set; }

    public int TotalClaimed { get; private set; }

    public int PerMemberQuota { get; private set; }

    public TicketClaimEventScopeType ScopeType { get; private set; }

    public Guid ScopeId { get; private set; }

    public Guid? TicketTemplateId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static Result<TicketClaimEvent> Create(
        Guid tenantId,
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int totalQuota,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventTenantRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventNameRequired);
        }

        if (name.Trim().Length > 128)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventNameTooLong);
        }

        if (startsAtUtc >= endsAtUtc)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidTimeWindow);
        }

        if (totalQuota < 1)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (perMemberQuota < 1)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (scopeId == Guid.Empty)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventScopeRequired);
        }

        return new TicketClaimEvent(
            Guid.NewGuid(),
            tenantId,
            name.Trim(),
            startsAtUtc,
            endsAtUtc,
            TicketClaimEventStatus.Draft,
            totalQuota,
            0,
            perMemberQuota,
            scopeType,
            scopeId,
            ticketTemplateId,
            utcNow,
            utcNow);
    }

    public Result UpdateInfo(
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int totalQuota,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended or TicketClaimEventStatus.SoldOut)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotEditable);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(GamingErrors.TicketClaimEventNameRequired);
        }

        if (name.Trim().Length > 128)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNameTooLong);
        }

        if (startsAtUtc >= endsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidTimeWindow);
        }

        if (totalQuota < 1 || perMemberQuota < 1)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (scopeId == Guid.Empty)
        {
            return Result.Failure(GamingErrors.TicketClaimEventScopeRequired);
        }

        if (Status == TicketClaimEventStatus.Active)
        {
            Name = name.Trim();
            EndsAtUtc = endsAtUtc;
        }
        else
        {
            Name = name.Trim();
            StartsAtUtc = startsAtUtc;
            EndsAtUtc = endsAtUtc;
            TotalQuota = totalQuota;
            PerMemberQuota = perMemberQuota;
            ScopeType = scopeType;
            ScopeId = scopeId;
            TicketTemplateId = ticketTemplateId;
        }

        if (TotalClaimed > TotalQuota)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result Activate(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Active)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyActive);
        }

        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            UpdatedAtUtc = utcNow;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        Status = TicketClaimEventStatus.Active;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result Disable(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        Status = TicketClaimEventStatus.Disabled;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result End(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        Status = TicketClaimEventStatus.Ended;
        EndsAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    public Result EnsureCanClaim(DateTime utcNow)
    {
        if (Status == TicketClaimEventStatus.Disabled)
        {
            return Result.Failure(GamingErrors.TicketClaimEventDisabled);
        }

        if (Status == TicketClaimEventStatus.Draft)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotActive);
        }

        if (Status == TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventEnded);
        }

        if (Status == TicketClaimEventStatus.SoldOut)
        {
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        if (utcNow < StartsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotStarted);
        }

        if (utcNow >= EndsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventEnded);
        }

        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            UpdatedAtUtc = utcNow;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        return Result.Success();
    }

    public Result IncreaseClaimed(int quantity, DateTime utcNow)
    {
        if (quantity <= 0)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (TotalClaimed + quantity > TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        TotalClaimed += quantity;
        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
        }

        UpdatedAtUtc = utcNow;
        return Result.Success();
    }
}
