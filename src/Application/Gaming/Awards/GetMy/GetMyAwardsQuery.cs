using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Awards.GetMy;

/// <summary>
/// 取得會員得獎列表查詢。
/// </summary>
public sealed record GetMyAwardsQuery(string? Status) : IQuery<IReadOnlyCollection<PrizeAwardDto>>;
