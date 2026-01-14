using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 獎項快照值物件，記錄期數當下的名稱/成本/有效天數等。
/// </summary>
public sealed record PrizeOption
{
    private PrizeOption(Guid? prizeId, string name, decimal cost, int? redeemValidDays, string? description)
    {
        PrizeId = prizeId;
        Name = name;
        Cost = cost;
        RedeemValidDays = redeemValidDays;
        Description = description;
    }

    private PrizeOption()
        : this(null, string.Empty, 0m, null, null)
    {
    }

    /// <summary>
    /// 舊獎品識別（選用），保留對應關係。
    /// </summary>
    public Guid? PrizeId { get; }

    /// <summary>
    /// 獎項名稱快照。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 獎項成本快照。
    /// </summary>
    public decimal Cost { get; }

    /// <summary>
    /// 兌獎有效天數快照（可覆蓋 Draw 設定）。
    /// </summary>
    public int? RedeemValidDays { get; }

    /// <summary>
    /// 獎項描述快照（選填）。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 建立獎項快照，提供基本欄位驗證。
    /// </summary>
    public static Result<PrizeOption> Create(
        string name,
        decimal cost,
        int? redeemValidDays,
        string? description,
        Guid? prizeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<PrizeOption>(GamingErrors.PrizeNameRequired);
        }

        if (cost < 0)
        {
            return Result.Failure<PrizeOption>(GamingErrors.PrizeCostInvalid);
        }

        if (redeemValidDays.HasValue && redeemValidDays.Value <= 0)
        {
            return Result.Failure<PrizeOption>(GamingErrors.PrizeRedeemValidDaysInvalid);
        }

        return new PrizeOption(prizeId, name.Trim(), cost, redeemValidDays, description?.Trim());
    }
}
