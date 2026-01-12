namespace Application.Gaming.Dtos;

public sealed record PrizeRuleDto(
    Guid Id,
    int MatchCount,
    Guid PrizeId,
    string PrizeName,
    bool IsActive,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo);
