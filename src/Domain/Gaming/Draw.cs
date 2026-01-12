using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 539 開獎期數的聚合根，負責管理期數生命週期與公平性證明資料。
/// </summary>
/// <remarks>
/// Domain 層只描述規則與狀態，不直接依賴基礎設施或外部服務。
/// </remarks>
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

    /// <summary>
    /// 租戶識別，隔離不同租戶的期數資料。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 售票開始時間（UTC），低於此時間不得購買。
    /// </summary>
    public DateTime SalesOpenAt { get; private set; }

    /// <summary>
    /// 售票截止時間（UTC），到點後必須停止收單。
    /// </summary>
    public DateTime SalesCloseAt { get; private set; }

    /// <summary>
    /// 開獎時間（UTC），用來決定何時揭露 ServerSeed。
    /// </summary>
    public DateTime DrawAt { get; private set; }

    /// <summary>
    /// 期數狀態，遵循 Scheduled → SalesOpen → SalesClosed → Settled/Cancelled 的約束。
    /// </summary>
    public DrawStatus Status { get; private set; }

    /// <summary>
    /// 已開獎的中獎號碼序列（持久化格式）。
    /// </summary>
    public string? WinningNumbers { get; private set; }

    /// <summary>
    /// Commit-Reveal 的 commit：先存 ServerSeedHash，避免伺服器事後修改。
    /// </summary>
    public string? ServerSeedHash { get; private set; }

    /// <summary>
    /// Commit-Reveal 的 reveal：開獎後揭露 ServerSeed 供外部驗證。
    /// </summary>
    public string? ServerSeed { get; private set; }

    /// <summary>
    /// RNG 使用的演算法名稱，讓驗證者能對應正確的 hash/HMAC 方法。
    /// </summary>
    public string? Algorithm { get; private set; }

    /// <summary>
    /// RNG 推導輸入（通常是 drawId 等），確保外部可重算。
    /// </summary>
    public string? DerivedInput { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新時間（UTC），用於狀態追蹤。
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 建立新期數並檢查時間邏輯，避免售票區間顛倒。
    /// </summary>
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

    /// <summary>
    /// 開放販售並寫入 ServerSeedHash（commit），避免事後質疑公平性。
    /// </summary>
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

    /// <summary>
    /// 關閉販售，確保開獎前不再新增票券。
    /// </summary>
    public void CloseSales(DateTime utcNow)
    {
        if (Status == DrawStatus.SalesOpen)
        {
            Status = DrawStatus.SalesClosed;
            UpdatedAt = utcNow;
        }
    }

    /// <summary>
    /// 執行開獎並寫入 proof，狀態轉為 Settled。
    /// </summary>
    /// <remarks>
    /// proof 包含 ServerSeed、Algorithm 與 DerivedInput，讓外部可重算中獎號碼。
    /// </remarks>
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

    /// <summary>
    /// 解析已儲存的號碼，失敗時回傳 null 以避免污染其他流程。
    /// </summary>
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
