using SharedKernel;

namespace Domain.Gaming;

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
        Numbers = numbers;
    }

    private TicketLine()
    {
    }

    public Guid TicketId { get; private set; }

    public int LineIndex { get; private set; }

    public string Numbers { get; private set; } = string.Empty;

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

        TicketLine line = new TicketLine(Guid.NewGuid(), ticketId, lineIndex, numbers.ToStorageString());
        return line;
    }

    public LotteryNumbers? GetNumbers()
    {
        Result<LotteryNumbers> parsed = LotteryNumbers.Parse(Numbers);
        return parsed.IsSuccess ? parsed.Value : null;
    }
}
