using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.PrizeRules.GetList;

/// <summary>
/// 取得中獎規則列表查詢。
/// </summary>
public sealed record GetPrizeRuleListQuery : IQuery<IReadOnlyCollection<PrizeRuleDto>>;
