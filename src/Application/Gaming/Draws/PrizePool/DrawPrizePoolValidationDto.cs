namespace Application.Gaming.Draws.PrizePool;

public sealed record DrawPrizePoolValidationDto(
    bool IsComplete,
    IReadOnlyCollection<MissingPrizePoolSlotDto> Missing);
