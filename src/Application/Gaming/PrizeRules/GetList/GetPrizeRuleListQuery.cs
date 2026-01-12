using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.PrizeRules.GetList;

public sealed record GetPrizeRuleListQuery : IQuery<IReadOnlyCollection<PrizeRuleDto>>;
