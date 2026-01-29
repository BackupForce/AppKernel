namespace Application.Gaming.Draws.PrizePool;

public sealed record PrizeOptionDto(
    Guid? PrizeId,
    string Name,
    decimal Cost,
    decimal PayoutAmount,
    int? RedeemValidDays,
    string? Description);
