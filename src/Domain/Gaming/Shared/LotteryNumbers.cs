using SharedKernel;

namespace Domain.Gaming.Shared;

/// <summary>
/// 539 遊戲的號碼值物件，維持 5-of-39 不重複的規則。
/// </summary>
/// <remarks>
/// 以值物件封裝驗證邏輯，避免其他層直接操作無效號碼。
/// </remarks>
public sealed class LotteryNumbers : IEquatable<LotteryNumbers>
{
    private const int RequiredCount = 5;
    private const int MinNumber = 1;
    private const int MaxNumber = 39;

    private LotteryNumbers(IReadOnlyList<int> numbers)
    {
        Numbers = numbers;
    }

    /// <summary>
    /// 已排序的投注號碼列表（升冪）。
    /// </summary>
    public IReadOnlyList<int> Numbers { get; }

    /// <summary>
    /// 建立號碼組合，遵循 5 個號碼、1~39 且不重複的規則。
    /// </summary>
    /// <remarks>
    /// 驗證失敗時回傳 Result.Failure，讓上層可以回傳明確錯誤碼。
    /// </remarks>
    public static Result<LotteryNumbers> Create(IEnumerable<int> numbers)
    {
        if (numbers is null)
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersRequired);
        }

        var normalized = numbers.Select(number => number).ToList();

        if (normalized.Count != RequiredCount)
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersCountInvalid);
        }

        if (normalized.Any(number => number < MinNumber || number > MaxNumber))
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersOutOfRange);
        }

        if (normalized.Distinct().Count() != RequiredCount)
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersDuplicated);
        }

        normalized.Sort();

        return new LotteryNumbers(normalized);
    }

    /// <summary>
    /// 從持久化格式解析（逗號分隔），不合法時回傳對應錯誤。
    /// </summary>
    public static Result<LotteryNumbers> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersRequired);
        }

        string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var numbers = new List<int>();
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int number))
            {
                return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersFormatInvalid);
            }

            numbers.Add(number);
        }

        return Create(numbers);
    }

    /// <summary>
    /// 轉換為持久化格式，供資料庫或訊息儲存。
    /// </summary>
    public string ToStorageString()
    {
        return string.Join(',', Numbers);
    }

    public bool Equals(LotteryNumbers? other)
    {
        if (other is null)
        {
            return false;
        }

        return Numbers.SequenceEqual(other.Numbers);
    }

    public override bool Equals(object? obj)
    {
        return obj is LotteryNumbers other && Equals(other);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (int number in Numbers)
        {
            hash = hash * 31 + number.GetHashCode();
        }

        return hash;
    }
}
