using Application.Gaming.Dtos;
using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetOpen;

/// <summary>
/// 取得可投注期數列表查詢。
/// </summary>
public sealed record GetOpenDrawsQuery(string? Status) : IQuery<IReadOnlyCollection<DrawSummaryDto>>;
