using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Prizes.GetList;

/// <summary>
/// 取得獎品列表查詢。
/// </summary>
public sealed record GetPrizeListQuery : IQuery<IReadOnlyCollection<PrizeDto>>;
