using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Tickets;

/// <summary>
/// 票券聚合根，代表一次發放或領取，可包含多注（多個 TicketLine）。
/// </summary>
/// <remarks>
/// 多注設計保留每一注的 LineIndex 以便結算與防重。
/// </remarks>
public sealed class Ticket : Entity
{
    private readonly List<TicketLine> _lines = new();

    private Ticket(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        Guid memberId,
        Guid? campaignId,
        Guid? ticketTemplateId,
        Guid? drawId,
        decimal? priceSnapshot,
        long? totalCost,
        DateTime issuedAtUtc,
        IssuedByType issuedByType,
        Guid? issuedByUserId,
        string? issuedReason,
        string? issuedNote,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        GameCode = gameCode;
        PlayTypeCode = playTypeCode;
        MemberId = memberId;
        CampaignId = campaignId;
        TicketTemplateId = ticketTemplateId;
        DrawId = drawId;
        PriceSnapshot = priceSnapshot;
        TotalCost = totalCost;
        IssuedAtUtc = issuedAtUtc;
        IssuedByType = issuedByType;
        IssuedByUserId = issuedByUserId;
        IssuedReason = issuedReason;
        IssuedNote = issuedNote;
        SubmissionStatus = TicketSubmissionStatus.NotSubmitted;
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
    /// 主要期數識別（相容舊查詢，已改由 TicketDraw 維護逐期關係）。
    /// </summary>
    public Guid? DrawId { get; private set; }

    /// <summary>
    /// 遊戲代碼（與期數一致）。
    /// </summary>
    public GameCode GameCode { get; private set; }

    /// <summary>
    /// 玩法代碼（本票券所屬玩法）。
    /// </summary>
    public PlayTypeCode PlayTypeCode { get; private set; }

    /// <summary>
    /// 購買者（會員）識別。
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// 活動識別（若由活動發放）。
    /// </summary>
    public Guid? CampaignId { get; private set; }

    /// <summary>
    /// 票種模板識別，對應發放時選用的 TicketTemplate。
    /// </summary>
    public Guid? TicketTemplateId { get; private set; }

    /// <summary>
    /// 票價快照，避免後續模板改價影響歷史稽核。
    /// </summary>
    public decimal? PriceSnapshot { get; private set; }

    /// <summary>
    /// 此票券總成本（所有 Line 加總），用於帳本扣點與報表。
    /// </summary>
    public long? TotalCost { get; private set; }

    /// <summary>
    /// 發放時間（UTC）。
    /// </summary>
    public DateTime IssuedAtUtc { get; private set; }

    /// <summary>
    /// 發放來源類型。
    /// </summary>
    public IssuedByType IssuedByType { get; private set; }

    /// <summary>
    /// 發放人員識別（選填）。
    /// </summary>
    public Guid? IssuedByUserId { get; private set; }

    /// <summary>
    /// 發放原因（選填）。
    /// </summary>
    public string? IssuedReason { get; private set; }

    /// <summary>
    /// 發放備註（選填）。
    /// </summary>
    public string? IssuedNote { get; private set; }

    /// <summary>
    /// 提交狀態。
    /// </summary>
    public TicketSubmissionStatus SubmissionStatus { get; private set; }

    /// <summary>
    /// 提交時間（UTC）。
    /// </summary>
    public DateTime? SubmittedAtUtc { get; private set; }

    /// <summary>
    /// 提交人員識別（選填）。
    /// </summary>
    public Guid? SubmittedByUserId { get; private set; }

    /// <summary>
    /// 提交用的客戶端參考（選填）。
    /// </summary>
    public string? SubmittedClientReference { get; private set; }

    /// <summary>
    /// 提交備註（選填）。
    /// </summary>
    public string? SubmittedNote { get; private set; }

    /// <summary>
    /// 作廢時間（UTC）。
    /// </summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>
    /// 作廢原因（選填）。
    /// </summary>
    public string? CancelledReason { get; private set; }

    /// <summary>
    /// 作廢人員識別（選填）。
    /// </summary>
    public Guid? CancelledByUserId { get; private set; }

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
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        Guid memberId,
        Guid? campaignId,
        Guid? ticketTemplateId,
        Guid? drawId,
        decimal? priceSnapshot,
        long? totalCost,
        DateTime issuedAtUtc,
        IssuedByType issuedByType,
        Guid? issuedByUserId,
        string? issuedReason,
        string? issuedNote,
        DateTime createdAt)
    {
        return new Ticket(
            Guid.NewGuid(),
            tenantId,
            gameCode,
            playTypeCode,
            memberId,
            campaignId,
            ticketTemplateId,
            drawId,
            priceSnapshot,
            totalCost,
            issuedAtUtc,
            issuedByType,
            issuedByUserId,
            issuedReason,
            issuedNote,
            createdAt);
    }

    /// <summary>
    /// 提交投注號碼，僅允許一次。
    /// </summary>
    public Result SubmitNumbers(
        LotteryNumbers numbers,
        DateTime utcNow,
        Guid? submittedByUserId,
        string? clientReference,
        string? note)
    {
        if (SubmissionStatus != TicketSubmissionStatus.NotSubmitted)
        {
            return Result.Failure(GamingErrors.TicketAlreadySubmitted);
        }

        if (_lines.Count > 0)
        {
            return Result.Failure(GamingErrors.TicketNumbersAlreadySubmitted);
        }

        Result<TicketLine> lineResult = TicketLine.Create(Id, 0, numbers);
        if (lineResult.IsFailure)
        {
            return Result.Failure(lineResult.Error);
        }

        _lines.Add(lineResult.Value);
        SubmissionStatus = TicketSubmissionStatus.Submitted;
        SubmittedAtUtc = utcNow;
        SubmittedByUserId = submittedByUserId;
        SubmittedClientReference = clientReference;
        SubmittedNote = note;

        return Result.Success();
    }

    /// <summary>
    /// 作廢票券。
    /// </summary>
    public void Cancel(Guid? cancelledByUserId, string? reason, DateTime utcNow)
    {
        SubmissionStatus = TicketSubmissionStatus.Cancelled;
        CancelledAtUtc = utcNow;
        CancelledReason = reason;
        CancelledByUserId = cancelledByUserId;
    }

    /// <summary>
    /// 加入一注（Line），LineIndex 由應用層維護以支援結算與防重。
    /// </summary>
    public void AddLine(TicketLine line)
    {
        _lines.Add(line);
    }
}
