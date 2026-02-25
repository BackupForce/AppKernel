namespace Application.Gaming.Draws.GetWinningNumbersByUid;

public sealed record DrawWinningNumbersDto(
    Guid DrawId,
    string Uid,
    DateTime DrawDateUtc,
    IReadOnlyCollection<int> WinningNumbers)
{
    public static DrawWinningNumbersDto Create(
        Guid drawId,
        string uid,
        DateTime drawDateUtc,
        int[] winningNumbers)
    {
        IReadOnlyCollection<int> readOnlyWinningNumbers = Array.AsReadOnly(winningNumbers);
        return new DrawWinningNumbersDto(drawId, uid, drawDateUtc, readOnlyWinningNumbers);
    }
}
