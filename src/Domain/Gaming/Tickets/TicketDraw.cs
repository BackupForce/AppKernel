using SharedKernel;

namespace Domain.Gaming.Tickets;

public sealed class TicketDraw : Entity
{
    private TicketDraw(
        Guid id,
        Guid tenantId,
        Guid ticketId,
        Guid drawId,
        TicketDrawParticipationStatus status,
        DateTime createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        TicketId = ticketId;
        DrawId = drawId;
        ParticipationStatus = status;
        CreatedAtUtc = createdAtUtc;
    }

    private TicketDraw()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid TicketId { get; private set; }

    public Guid DrawId { get; private set; }

    public TicketDrawParticipationStatus ParticipationStatus { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? EvaluatedAtUtc { get; private set; }

    public DateTime? SettledAtUtc { get; private set; }

    public DateTime? RedeemedAtUtc { get; private set; }

    public static TicketDraw Create(Guid tenantId, Guid ticketId, Guid drawId, DateTime utcNow)
    {
        return new TicketDraw(Guid.NewGuid(), tenantId, ticketId, drawId, TicketDrawParticipationStatus.Pending, utcNow);
    }

    public void MarkActive(DateTime utcNow)
    {
        ParticipationStatus = TicketDrawParticipationStatus.Active;
        EvaluatedAtUtc = utcNow;
    }

    public void MarkInvalid(DateTime utcNow)
    {
        ParticipationStatus = TicketDrawParticipationStatus.Invalid;
        EvaluatedAtUtc = utcNow;
    }

    public void MarkSettled(DateTime utcNow)
    {
        ParticipationStatus = TicketDrawParticipationStatus.Settled;
        SettledAtUtc = utcNow;
    }

    public void MarkRedeemed(DateTime utcNow)
    {
        ParticipationStatus = TicketDrawParticipationStatus.Redeemed;
        RedeemedAtUtc = utcNow;
    }

    public void Cancel(DateTime utcNow)
    {
        ParticipationStatus = TicketDrawParticipationStatus.Cancelled;
        EvaluatedAtUtc = utcNow;
    }
}
