using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Catalog;

/// <summary>
/// 玩法代碼值物件，避免跨遊戲混用。
/// </summary>
public readonly record struct PlayTypeCode
{
    public PlayTypeCode(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// 正規化後的代碼字串。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 建立玩法代碼，空值會回傳錯誤。
    /// </summary>
    public static Result<PlayTypeCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PlayTypeCode>(GamingErrors.PlayTypeCodeRequired);
        }

        return new PlayTypeCode(value);
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
