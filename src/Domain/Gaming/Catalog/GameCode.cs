using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Catalog;

/// <summary>
/// 遊戲代碼值物件，統一大小寫與字串格式。
/// </summary>
public readonly record struct GameCode
{
    public GameCode(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// 正規化後的代碼字串。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 建立遊戲代碼，空值會回傳錯誤。
    /// </summary>
    public static Result<GameCode> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<GameCode>(GamingErrors.GameCodeRequired);
        }

        return new GameCode(value);
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
