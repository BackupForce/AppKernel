namespace Application.Gaming.Draws.GetWinningNumbersByUid;

public sealed record DrawWinningNumbersDto(
    Guid DrawId,
    DateTime DrawDateUtc,
    IReadOnlyCollection<int> WinningNumbers)
{
    public static DrawWinningNumbersDto Create(
        Guid drawId,
        DateTime drawDateUtc,
        string winningNumbers)
    {
        IReadOnlyCollection<int> readOnlyWinningNumbers = ParseWinningNumbers(winningNumbers);
        return new DrawWinningNumbersDto(drawId,  drawDateUtc, readOnlyWinningNumbers);
    }

    private static int[] ParseWinningNumbers(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<int>();
        }

        string[] tokens = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);

        int[] numbers = new int[tokens.Length];

        for (int i = 0; i < tokens.Length; i++)
        {
            if (!int.TryParse(tokens[i], out int value))
            {
                throw new InvalidOperationException(
                    $"Invalid winning number format: '{tokens[i]}'");
            }

            numbers[i] = value;
        }

        return numbers;
    }
}
