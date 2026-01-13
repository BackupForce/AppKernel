using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.PrizeMappings.Get;

/// <summary>
/// 取得期數獎項對應設定查詢。
/// </summary>
public sealed record GetDrawPrizeMappingsQuery(Guid DrawId) : IQuery<IReadOnlyCollection<DrawPrizeMappingDto>>;
