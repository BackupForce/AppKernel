using SharedKernel;

namespace Domain.Gaming;

public sealed class Ticket : Entity
{
    private readonly List<TicketLine> _lines = new();

    private Ticket(
        Guid id,
        Guid tenantId,
        Guid drawId,
        Guid memberId,
        long totalCost,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        MemberId = memberId;
        TotalCost = totalCost;
        CreatedAt = createdAt;
    }

    private Ticket()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawId { get; private set; }

    public Guid MemberId { get; private set; }

    public long TotalCost { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<TicketLine> Lines => _lines.AsReadOnly();

    public static Ticket Create(
        Guid tenantId,
        Guid drawId,
        Guid memberId,
        long totalCost,
        DateTime createdAt)
    {
        return new Ticket(Guid.NewGuid(), tenantId, drawId, memberId, totalCost, createdAt);
    }

    public void AddLine(TicketLine line)
    {
        _lines.Add(line);
    }
}
