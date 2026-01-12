using SharedKernel;

namespace Domain.Gaming;

public sealed class Draw : Entity
{
    private Draw(
        Guid id,
        Guid tenantId,
        DateTime salesOpenAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        DrawStatus status,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        SalesOpenAt = salesOpenAt;
        SalesCloseAt = salesCloseAt;
        DrawAt = drawAt;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private Draw()
    {
    }

    public Guid TenantId { get; private set; }

    public DateTime SalesOpenAt { get; private set; }

    public DateTime SalesCloseAt { get; private set; }

    public DateTime DrawAt { get; private set; }

    public DrawStatus Status { get; private set; }

    public string? WinningNumbers { get; private set; }

    // 中文註解：commit-reveal proof 欄位，ServerSeedHash 於販售開始時寫入，ServerSeed 於開獎揭露。
    public string? ServerSeedHash { get; private set; }

    public string? ServerSeed { get; private set; }

    public string? Algorithm { get; private set; }

    public string? DerivedInput { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Draw> Create(
        Guid tenantId,
        DateTime salesOpenAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        DrawStatus initialStatus,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Draw>(GamingErrors.DrawNotFound);
        }

        if (salesOpenAt >= salesCloseAt || salesCloseAt > drawAt)
        {
            return Result.Failure<Draw>(GamingErrors.DrawTimeInvalid);
        }

        Draw draw = new Draw(
            Guid.NewGuid(),
            tenantId,
            salesOpenAt,
            salesCloseAt,
            drawAt,
            initialStatus,
            utcNow,
            utcNow);

        return draw;
    }

    public void OpenSales(string serverSeedHash, DateTime utcNow)
    {
        if (Status == DrawStatus.Scheduled)
        {
            Status = DrawStatus.SalesOpen;
        }

        if (string.IsNullOrWhiteSpace(ServerSeedHash))
        {
            ServerSeedHash = serverSeedHash;
        }

        UpdatedAt = utcNow;
    }

    public void CloseSales(DateTime utcNow)
    {
        if (Status == DrawStatus.SalesOpen)
        {
            Status = DrawStatus.SalesClosed;
            UpdatedAt = utcNow;
        }
    }

    public void Execute(
        LotteryNumbers winningNumbers,
        string serverSeed,
        string algorithm,
        string derivedInput,
        DateTime utcNow)
    {
        WinningNumbers = winningNumbers.ToStorageString();
        ServerSeed = serverSeed;
        Algorithm = algorithm;
        DerivedInput = derivedInput;
        Status = DrawStatus.Settled;
        UpdatedAt = utcNow;
    }

    public LotteryNumbers? GetWinningNumbers()
    {
        if (string.IsNullOrWhiteSpace(WinningNumbers))
        {
            return null;
        }

        Result<LotteryNumbers> parsed = LotteryNumbers.Parse(WinningNumbers);
        return parsed.IsSuccess ? parsed.Value : null;
    }
}
