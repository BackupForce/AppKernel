using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Deactivate;

/// <summary>
/// 停用中獎規則命令。
/// </summary>
public sealed record DeactivatePrizeRuleCommand(Guid RuleId) : ICommand;
