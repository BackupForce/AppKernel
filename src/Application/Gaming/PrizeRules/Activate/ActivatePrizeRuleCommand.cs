using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Activate;

public sealed record ActivatePrizeRuleCommand(Guid RuleId) : ICommand;
