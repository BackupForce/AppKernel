using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 獎級值物件，對應玩法規則固定 tiers。
/// </summary>
public readonly record struct PrizeTier
{
    public PrizeTier(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// 正規化後的獎級代碼。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 建立獎級代碼，空值會回傳錯誤。
    /// </summary>
    public static Result<PrizeTier> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PrizeTier>(GamingErrors.PrizeTierRequired);
        }

        return new PrizeTier(value);
    }

    /// <summary>
    /// 正規化字串格式（Trim + UpperInvariant）。
    /// </summary>
    public static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
