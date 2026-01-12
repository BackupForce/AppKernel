using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Update;

public sealed record UpdatePrizeRuleCommand(
    Guid RuleId,
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo) : ICommand;
