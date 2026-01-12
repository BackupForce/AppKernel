namespace Domain.Gaming.Services;

public static class Lottery539MatchCalculator
{
    // 中文註解：只計算命中顆數，純邏輯無外部依賴。
    public static int CalculateMatchedCount(IReadOnlyCollection<int> winningNumbers, IReadOnlyCollection<int> lineNumbers)
    {
        HashSet<int> winningSet = new HashSet<int>(winningNumbers);
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
