using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 票券聚合根，代表一次購買，可包含多注（多個 TicketLine）。
/// </summary>
/// <remarks>
/// 多注設計可降低交易與記錄成本，並保留每一注的 LineIndex 以便結算與防重。
/// </remarks>
public sealed class Ticket : Entity
{
    private readonly List<TicketLine> _lines = new();

    private Ticket(
        Guid id,
        Guid tenantId,
        Guid drawId,
        Guid memberId,
        Guid ticketTemplateId,
        decimal priceSnapshot,
        long totalCost,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        MemberId = memberId;
        TicketTemplateId = ticketTemplateId;
        PriceSnapshot = priceSnapshot;
        TotalCost = totalCost;
        CreatedAt = createdAt;
    }

    private Ticket()
    {
    }

    /// <summary>
    /// 租戶識別，隔離不同租戶的票券。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 對應期數。
    /// </summary>
    public Guid DrawId { get; private set; }

    /// <summary>
    /// 購買者（會員）識別。
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// 票種模板識別，對應下單時選用的 TicketTemplate。
    /// </summary>
    public Guid TicketTemplateId { get; private set; }

    /// <summary>
    /// 票價快照，避免後續模板改價影響歷史稽核。
    /// </summary>
    public decimal PriceSnapshot { get; private set; }

    /// <summary>
    /// 此票券總成本（所有 Line 加總），用於帳本扣點與報表。
    /// </summary>
    public long TotalCost { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 票券內的所有投注注數。
    /// </summary>
    public IReadOnlyCollection<TicketLine> Lines => _lines.AsReadOnly();

    /// <summary>
    /// 建立票券主體，Line 由外部逐一加入以維持一致性與驗證。
    /// </summary>
    public static Ticket Create(
        Guid tenantId,
        Guid drawId,
        Guid memberId,
        Guid ticketTemplateId,
        decimal priceSnapshot,
        long totalCost,
        DateTime createdAt)
    {
        return new Ticket(Guid.NewGuid(), tenantId, drawId, memberId, ticketTemplateId, priceSnapshot, totalCost, createdAt);
    }

    /// <summary>
    /// 加入一注（Line），LineIndex 由應用層維護以支援結算與防重。
    /// </summary>
    public void AddLine(TicketLine line)
    {
        _lines.Add(line);
    }
}
