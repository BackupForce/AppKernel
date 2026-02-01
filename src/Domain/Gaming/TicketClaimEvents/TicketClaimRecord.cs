using SharedKernel;

namespace Domain.Gaming.TicketClaimEvents;

public sealed class TicketClaimRecord : Entity
{
    private TicketClaimRecord(
        Guid id,
        Guid tenantId,
        Guid eventId,
        Guid memberId,
        int quantity,
        string? idempotencyKey,
        string? issuedTicketIds,
        DateTime claimedAtUtc) : base(id)
    {
        TenantId = tenantId;
        EventId = eventId;
        MemberId = memberId;
        Quantity = quantity;
        IdempotencyKey = idempotencyKey;
        IssuedTicketIds = issuedTicketIds;
        ClaimedAtUtc = claimedAtUtc;
    }

    private TicketClaimRecord()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid MemberId { get; private set; }

    public int Quantity { get; private set; }

    public string? IdempotencyKey { get; private set; }

    public string? IssuedTicketIds { get; private set; }

    public DateTime ClaimedAtUtc { get; private set; }

    public static TicketClaimRecord Create(
        Guid tenantId,
        Guid eventId,
        Guid memberId,
        int quantity,
        string? idempotencyKey,
        string? issuedTicketIds,
        DateTime claimedAtUtc)
    {
        return new TicketClaimRecord(
            Guid.NewGuid(),
            tenantId,
            eventId,
            memberId,
            quantity,
            string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
            issuedTicketIds,
            claimedAtUtc);
    }
}
