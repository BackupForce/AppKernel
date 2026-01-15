namespace Domain.Gaming.Rules;

/// <summary>
/// 539 命中計算器，純邏輯無外部依賴。
/// </summary>
public static class Lottery539MatchCalculator
{
    /// <summary>
    /// 計算投注與中獎號碼的命中顆數。
    /// </summary>
    public static int CalculateMatchedCount(IReadOnlyCollection<int> winningNumbers, IReadOnlyCollection<int> lineNumbers)
    {
        var winningSet = new HashSet<int>(winningNumbers);
        int matched = 0;

        foreach (int number in lineNumbers)
        {
            if (winningSet.Contains(number))
            {
                matched++;
            }
        }

        return matched;
    }
}
