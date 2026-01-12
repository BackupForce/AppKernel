using SharedKernel;

namespace Domain.Gaming;

public sealed class LotteryNumbers : IEquatable<LotteryNumbers>
{
    private const int RequiredCount = 5;
    private const int MinNumber = 1;
    private const int MaxNumber = 39;

    private LotteryNumbers(IReadOnlyList<int> numbers)
    {
        Numbers = numbers;
    }

    public IReadOnlyList<int> Numbers { get; }

    public static Result<LotteryNumbers> Create(IEnumerable<int> numbers)
    {
        if (numbers is null)
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersRequired);
        }

        List<int> normalized = numbers.Select(number => number).ToList();

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

    public static Result<LotteryNumbers> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersRequired);
        }

        string[] parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<int> numbers = new List<int>();
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
