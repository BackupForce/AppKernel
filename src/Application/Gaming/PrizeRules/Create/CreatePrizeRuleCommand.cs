using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Create;

public sealed record CreatePrizeRuleCommand(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo) : ICommand<Guid>;
