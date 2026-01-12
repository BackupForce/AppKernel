using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Create;

/// <summary>
/// 建立中獎規則命令。
/// </summary>
public sealed record CreatePrizeRuleCommand(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo) : ICommand<Guid>;
