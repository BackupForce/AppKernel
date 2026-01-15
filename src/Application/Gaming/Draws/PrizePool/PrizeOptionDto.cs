namespace Application.Gaming.Draws.PrizePool;

public sealed record PrizeOptionDto(
    Guid? PrizeId,
    string Name,
    decimal Cost,
    int? RedeemValidDays,
    string? Description);
