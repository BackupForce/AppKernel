using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Tickets;

/// <summary>
/// 票券內的一注投注，持有固定序號與投注號碼。
/// </summary>
/// <remarks>
/// LineIndex 用於結算與防重（TicketId + LineIndex），避免同一注被重複結算。
/// </remarks>
public sealed class TicketLine : Entity
{
    private TicketLine(
        Guid id,
        Guid ticketId,
        int lineIndex,
        string numbers) : base(id)
    {
        TicketId = ticketId;
        LineIndex = lineIndex;
        NumbersRaw = numbers;
    }

    private TicketLine()
    {
    }

    /// <summary>
    /// 所屬票券識別。
    /// </summary>
    public Guid TicketId { get; private set; }

    /// <summary>
    /// 在票券中的序號，需穩定不可變，以支援 idempotency。
    /// </summary>
    public int LineIndex { get; private set; }

    /// <summary>
    /// 投注號碼的儲存格式（逗號分隔）。
    /// 僅供 persistence 使用。
    /// </summary>
    public string NumbersRaw { get; private set; } = string.Empty;

    /// <summary>
    /// 建立投注注數，驗證 TicketId 與 LineIndex 合法性。
    /// </summary>
    public static Result<TicketLine> Create(Guid ticketId, int lineIndex, LotteryNumbers numbers)
    {
        if (ticketId == Guid.Empty)
        {
            return Result.Failure<TicketLine>(GamingErrors.TicketLineInvalid);
        }

        if (lineIndex < 0)
        {
            return Result.Failure<TicketLine>(GamingErrors.TicketLineInvalid);
        }

        var line = new TicketLine(Guid.NewGuid(), ticketId, lineIndex, numbers.ToStorageString());
        return line;
    }

    /// <summary>
    /// 解析儲存的投注號碼為領域物件。
    /// </summary>
    public LotteryNumbers? ParseNumbers()
    {
        Result<LotteryNumbers> parsed = LotteryNumbers.Parse(NumbersRaw);
        return parsed.IsSuccess ? parsed.Value : null;
    }
}
