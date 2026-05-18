using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.TicketClaimEvents;

public sealed class TicketClaimMemberCounter : Entity
{
    private TicketClaimMemberCounter(
        Guid eventId,
        Guid memberId,
        int claimedCount,
        DateTime updatedAtUtc)
    {
        EventId = eventId;
        MemberId = memberId;
        ClaimedCount = claimedCount;
        UpdatedAtUtc = updatedAtUtc;
    }

    private TicketClaimMemberCounter()
    {
    }

    public Guid EventId { get; private set; }

    public Guid MemberId { get; private set; }

    public int ClaimedCount { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static TicketClaimMemberCounter Create(Guid eventId, Guid memberId, DateTime utcNow)
    {
        return new TicketClaimMemberCounter(eventId, memberId, 0, utcNow);
    }

    public Result Increase(int quantity, int perMemberQuota, DateTime utcNow)
    {
        if (quantity <= 0)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (ClaimedCount + quantity > perMemberQuota)
        {
            return Result.Failure(GamingErrors.TicketClaimEventMemberQuotaExceeded);
        }

        ClaimedCount += quantity;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }
}
