using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Update;

/// <summary>
/// 更新中獎規則命令。
/// </summary>
public sealed record UpdatePrizeRuleCommand(
    Guid RuleId,
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo) : ICommand;
