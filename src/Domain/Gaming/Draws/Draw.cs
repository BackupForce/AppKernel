using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Draws;

/// <summary>
/// 539 開獎期數的聚合根，負責管理期數生命週期與公平性證明資料。
/// </summary>
/// <remarks>
/// Domain 層只描述規則與狀態，不直接依賴基礎設施或外部服務。
/// </remarks>
public sealed class Draw : Entity
{
    private readonly List<DrawEnabledPlayType> _enabledPlayTypes = new();
    private readonly List<DrawPrizePoolItem> _prizePool = new();

    private Draw(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        string drawCode,
        DateTime salesStartAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        int? redeemValidDays,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        GameCode = gameCode;
        DrawCode = drawCode;
        SalesOpenAt = salesStartAt;
        SalesCloseAt = salesCloseAt;
        DrawAt = drawAt;
        RedeemValidDays = redeemValidDays;
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
    /// 遊戲代碼，代表期數對應的遊戲類型。
    /// </summary>
    public GameCode GameCode { get; private set; }

    /// <summary>
    /// 售票開始時間（UTC），低於此時間不得購買。
    /// </summary>
    public DateTime SalesOpenAt { get; private set; }

    /// <summary>
    /// 人類可讀的期數代碼，例如：539-2601001。
    /// </summary>
    public string DrawCode { get; private set; }

    /// <summary>
    /// 售票起始時間別名（對應 SalesOpenAt），用於對外語意一致。
    /// </summary>
    public DateTime SalesStartAt => SalesOpenAt;

    /// <summary>
    /// 售票截止時間（UTC），到點後必須停止收單。
    /// </summary>
    public DateTime SalesCloseAt { get; private set; }

    /// <summary>
    /// 開獎時間（UTC），用來決定何時揭露 ServerSeed。
    /// </summary>
    public DateTime DrawAt { get; private set; }

    /// <summary>
    /// 期數狀態（相容用途）。
    /// </summary>
    /// <remarks>
    /// 狀態請以 <see cref="GetEffectiveStatus"/> 推導，Status 僅供 EF 還原與相容用途。
    /// </summary>
    public DrawStatus Status { get; private set; }

    /// <summary>
    /// 已開獎的中獎號碼（持久化格式）。
    /// 僅供儲存與還原使用。
    /// </summary>
    public string? WinningNumbersRaw { get; private set; }

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
    /// 手動封盤旗標，避免人工提前停止下注。
    /// </summary>
    public bool IsManuallyClosed { get; private set; }

    /// <summary>
    /// 手動封盤時間（UTC）。
    /// </summary>
    public DateTime? ManualCloseAt { get; private set; }

    /// <summary>
    /// 手動封盤原因，由後台輸入以利稽核。
    /// </summary>
    public string? ManualCloseReason { get; private set; }

    /// <summary>
    /// 實際開獎時間（UTC）。
    /// </summary>
    public DateTime? DrawnAt { get; private set; }

    /// <summary>
    /// 派彩/結算完成時間（UTC）。
    /// </summary>
    public DateTime? SettledAtUtc { get; private set; }

    /// <summary>
    /// 兌獎有效天數（若 PrizeRule 未指定，則以此為準）。
    /// </summary>
    public int? RedeemValidDays { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新時間（UTC），用於狀態追蹤。
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 來源模板識別（僅供稽核）。
    /// </summary>
    public Guid? SourceTemplateId { get; private set; }

    /// <summary>
    /// 來源模板版本（僅供稽核）。
    /// </summary>
    public int? SourceTemplateVersion { get; private set; }

    /// <summary>
    /// 本期啟用的玩法列表（只讀）。
    /// </summary>
    public IReadOnlyCollection<PlayTypeCode> EnabledPlayTypes =>
        _enabledPlayTypes.Select(item => item.PlayTypeCode).ToList();

    /// <summary>
    /// 供 EF 追蹤的玩法清單。
    /// </summary>
    public IReadOnlyCollection<DrawEnabledPlayType> EnabledPlayTypeItems => _enabledPlayTypes;

    /// <summary>
    /// 本期獎項配置（玩法 + 獎級）。
    /// </summary>
    public IReadOnlyCollection<DrawPrizePoolItem> PrizePoolItems => _prizePool;

    /// <summary>
    /// 建立新期數並檢查時間邏輯，避免售票區間顛倒。
    /// </summary>
    public static Result<Draw> Create(
        Guid tenantId,
        GameCode gameCode,
        string drawCode,
        DateTime salesStartAt,
        DateTime salesCloseAt,
        DateTime drawAt,
        int? redeemValidDays,
        DateTime utcNow,
        PlayRuleRegistry registry)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Draw>(GamingErrors.DrawNotFound);
        }

        if (string.IsNullOrWhiteSpace(gameCode.Value))
        {
            return Result.Failure<Draw>(GamingErrors.GameCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(drawCode))
        {
            return Result.Failure<Draw>(GamingErrors.DrawCodeRequired);
        }

        if (redeemValidDays.HasValue && redeemValidDays.Value <= 0)
        {
            return Result.Failure<Draw>(GamingErrors.DrawRedeemValidDaysInvalid);
        }

        if (salesStartAt >= salesCloseAt || salesCloseAt > drawAt)
        {
            return Result.Failure<Draw>(GamingErrors.DrawTimeInvalid);
        }

        var draw = new Draw(
            Guid.NewGuid(),
            tenantId,
            gameCode,
            drawCode,
            salesStartAt,
            salesCloseAt,
            drawAt,
            redeemValidDays,
            utcNow,
            utcNow);

        draw.EnsurePrizePoolSlotsInitialized(registry);

        return draw;
    }

    /// <summary>
    /// 啟用本期可販售的玩法。
    /// </summary>
    public Result EnablePlayTypes(IEnumerable<PlayTypeCode> playTypes, PlayRuleRegistry registry)
    {
        IReadOnlyCollection<PlayTypeCode> allowed = registry.GetAllowedPlayTypes(GameCode);

        foreach (PlayTypeCode playType in playTypes)
        {
            if (!allowed.Contains(playType))
            {
                return Result.Failure(GamingErrors.PlayTypeNotAllowed);
            }

            bool exists = _enabledPlayTypes.Any(item => item.PlayTypeCode == playType);
            if (exists)
            {
                return Result.Failure(GamingErrors.PlayTypeAlreadyEnabled);
            }

            var item = DrawEnabledPlayType.Create(TenantId, Id, playType);
            _enabledPlayTypes.Add(item);
        }

        EnsurePrizePoolSlotsInitialized(registry);

        return Result.Success();
    }

    /// <summary>
    /// 套用期數模板內容，將模板玩法與獎項複製到期數。
    /// </summary>
    public Result ApplyTemplate(
        DrawTemplate template,
        PlayRuleRegistry registry,
        DateTime utcNow)
    {
        if (template.GameCode != GameCode)
        {
            return Result.Failure(GamingErrors.DrawTemplateGameCodeMismatch);
        }

        List<PlayTypeCode> playTypes = template.PlayTypes
            .Select(item => item.PlayTypeCode)
            .ToList();

        Result enableResult = EnablePlayTypes(playTypes, registry);
        if (enableResult.IsFailure)
        {
            return enableResult;
        }

        foreach (DrawTemplatePrizeTier tier in template.PrizeTiers)
        {
            Result configureResult = ConfigurePrizeOption(
                tier.PlayTypeCode,
                tier.Tier,
                tier.Option,
                registry);
            if (configureResult.IsFailure)
            {
                return configureResult;
            }
        }

        SourceTemplateId = template.Id;
        SourceTemplateVersion = template.Version;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// 設定本期獎項配置（玩法 + 獎級）。
    /// </summary>
    public Result ConfigurePrizeOption(
        PlayTypeCode playType,
        PrizeTier tier,
        PrizeOption option,
        PlayRuleRegistry registry)
    {
        bool enabled = _enabledPlayTypes.Any(item => item.PlayTypeCode == playType);
        if (!enabled)
        {
            return Result.Failure(GamingErrors.TicketPlayTypeNotEnabled);
        }

        IPlayRule rule = registry.GetRule(GameCode, playType);
        if (!rule.GetTiers().Contains(tier))
        {
            return Result.Failure(GamingErrors.PrizeTierNotAllowed);
        }

        DrawPrizePoolItem? existing = _prizePool.Find(item => item.PlayTypeCode == playType && item.Tier == tier);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.PrizePoolSlotMissing);
        }

        existing.Update(option);

        return Result.Success();
    }

    /// <summary>
    /// 結算前檢查獎項配置是否完整。
    /// </summary>
    public Result EnsurePrizePoolCompleteForSettlement(PlayRuleRegistry registry)
    {
        foreach (DrawEnabledPlayType enabled in _enabledPlayTypes)
        {
            IPlayRule rule = registry.GetRule(GameCode, enabled.PlayTypeCode);
            foreach (PrizeTier tier in rule.GetTiers())
            {
                DrawPrizePoolItem? slot = _prizePool.Find(item => item.PlayTypeCode == enabled.PlayTypeCode && item.Tier == tier);
                if (slot is null)
                {
                    return Result.Failure(GamingErrors.PrizePoolSlotMissing);
                }

                if (slot.Option is null)
                {
                    return Result.Failure(GamingErrors.PrizePoolNotConfigured);
                }
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// 取得指定玩法與獎級的獎項設定。
    /// </summary>
    public PrizeOption? FindPrizeOption(PlayTypeCode playType, PrizeTier tier)
    {
        return _prizePool.Find(item => item.PlayTypeCode == playType && item.Tier == tier)?.Option;
    }

    private void EnsurePrizePoolSlotsInitialized(PlayRuleRegistry registry)
    {
        foreach (DrawEnabledPlayType enabled in _enabledPlayTypes)
        {
            IPlayRule rule = registry.GetRule(GameCode, enabled.PlayTypeCode);
            foreach (PrizeTier tier in rule.GetTiers())
            {
                bool exists = _prizePool.Any(item => item.PlayTypeCode == enabled.PlayTypeCode && item.Tier == tier);
                if (exists)
                {
                    continue;
                }

                _prizePool.Add(DrawPrizePoolItem.CreateEmpty(TenantId, Id, enabled.PlayTypeCode, tier));
            }
        }
    }

    /// <summary>
    /// 開放販售並寫入 ServerSeedHash（commit），避免事後質疑公平性。
    /// </summary>
    public void OpenSales(string serverSeedHash, DateTime utcNow)
    {
        DrawStatus status = GetEffectiveStatus(utcNow);
        if (status != DrawStatus.Scheduled && status != DrawStatus.SalesOpen)
        {
            return;
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
        if (utcNow < SalesOpenAt)
        {
            return;
        }

        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 執行開獎並寫入中獎號碼與 proof。
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
        WinningNumbersRaw = winningNumbers.ToStorageString();
        ServerSeed = serverSeed;
        Algorithm = algorithm;
        DerivedInput = derivedInput;
        DrawnAt = utcNow;
        IsManuallyClosed = false;
        ManualCloseAt = null;
        ManualCloseReason = null;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 手動封盤，僅記錄狀態與時間，驗證邏輯由應用層控制。
    /// </summary>
    public void CloseManually(string? reason, DateTime utcNow)
    {
        Status = DrawStatus.SalesClosed;
        IsManuallyClosed = true;
        ManualCloseAt = utcNow;
        ManualCloseReason = reason;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 手動解封，恢復自動販售判斷。
    /// </summary>
    public Result Reopen(DateTime utcNow)
    {
        if (DrawnAt.HasValue)
        {
            return Result.Failure(GamingErrors.DrawAlreadySettled);
        }

        Status = DrawStatus.SalesOpen;
        IsManuallyClosed = false;
        ManualCloseAt = null;
        ManualCloseReason = null;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// 判斷是否在有效售票視窗內（含手動封盤判斷）。
    /// </summary>
    public bool IsWithinSalesWindow(DateTime utcNow)
    {
        return GetEffectiveStatus(utcNow) == DrawStatus.SalesOpen;
    }

    /// <summary>
    /// 判斷是否已封盤（包含手動封盤或售票時間已結束）。
    /// </summary>
    public bool IsEffectivelyClosed(DateTime utcNow)
    {
        DrawStatus status = GetEffectiveStatus(utcNow);
        return status == DrawStatus.SalesClosed || status == DrawStatus.Drawn || status == DrawStatus.Settled;
    }

    /// <summary>
    /// 判斷是否在純時間的售票視窗內（不含手動封盤判斷）。
    /// </summary>
    public bool IsWithinSalesTimeRange(DateTime utcNow)
    {
        return utcNow >= SalesOpenAt && utcNow < SalesCloseAt;
    }

    /// <summary>
    /// 依照時間與手動封盤旗標推導期數狀態。
    /// </summary>
    public DrawStatus GetEffectiveStatus(DateTime utcNow)
    {
        if (Status == DrawStatus.Cancelled)
        {
            return DrawStatus.Cancelled;
        }

        if (SettledAtUtc.HasValue)
        {
            return DrawStatus.Settled;
        }

        if (DrawnAt.HasValue || !string.IsNullOrWhiteSpace(WinningNumbersRaw))
        {
            return DrawStatus.Drawn;
        }

        if (IsManuallyClosed)
        {
            return DrawStatus.SalesClosed;
        }

        if (utcNow < SalesOpenAt)
        {
            return DrawStatus.Scheduled;
        }

        if (utcNow >= SalesOpenAt && utcNow < SalesCloseAt)
        {
            return DrawStatus.SalesOpen;
        }

        return DrawStatus.SalesClosed;
    }

    /// <summary>
    /// 標記結算完成，代表派彩流程已全部完成。
    /// </summary>
    public void MarkSettled(DateTime nowUtc)
    {
        if (Status == DrawStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled draw cannot be settled.");
        }

        if (!DrawnAt.HasValue && string.IsNullOrWhiteSpace(WinningNumbersRaw))
        {
            throw new InvalidOperationException("Draw must be drawn before settlement.");
        }

        if (SettledAtUtc.HasValue)
        {
            return;
        }

        SettledAtUtc = nowUtc;
        UpdatedAt = nowUtc;
    }

    /// <summary>
    /// 解析已儲存的號碼，失敗時回傳 null 以避免污染其他流程。
    /// </summary>
    public LotteryNumbers? ParseWinningNumbers()
    {
        if (string.IsNullOrWhiteSpace(WinningNumbersRaw))
        {
            return null;
        }

        Result<LotteryNumbers> parsed = LotteryNumbers.Parse(WinningNumbersRaw);
        return parsed.IsSuccess ? parsed.Value : null;
    }
}
