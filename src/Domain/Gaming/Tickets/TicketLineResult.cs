using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Tickets;

public sealed class TicketLineResult : Entity
{
    private TicketLineResult(
        Guid id,
        Guid tenantId,
        Guid ticketId,
        Guid drawId,
        int lineIndex,
        PrizeTier prizeTier,
        decimal payout,
        DateTime settledAtUtc) : base(id)
    {
        TenantId = tenantId;
        TicketId = ticketId;
        DrawId = drawId;
        LineIndex = lineIndex;
        PrizeTier = prizeTier;
        Payout = payout;
        SettledAtUtc = settledAtUtc;
    }

    private TicketLineResult()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid TicketId { get; private set; }

    public Guid DrawId { get; private set; }

    public int LineIndex { get; private set; }

    public PrizeTier PrizeTier { get; private set; }

    public decimal Payout { get; private set; }

    public DateTime SettledAtUtc { get; private set; }

    public static TicketLineResult Create(
        Guid tenantId,
        Guid ticketId,
        Guid drawId,
        int lineIndex,
        PrizeTier prizeTier,
        decimal payout,
        DateTime settledAtUtc)
    {
        return new TicketLineResult(Guid.NewGuid(), tenantId, ticketId, drawId, lineIndex, prizeTier, payout, settledAtUtc);
    }
}
