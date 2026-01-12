using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Activate;

/// <summary>
/// 啟用中獎規則命令。
/// </summary>
public sealed record ActivatePrizeRuleCommand(Guid RuleId) : ICommand;
