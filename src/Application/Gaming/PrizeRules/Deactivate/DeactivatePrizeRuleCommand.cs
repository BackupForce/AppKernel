using Application.Abstractions.Messaging;

namespace Application.Gaming.PrizeRules.Deactivate;

public sealed record DeactivatePrizeRuleCommand(Guid RuleId) : ICommand;
