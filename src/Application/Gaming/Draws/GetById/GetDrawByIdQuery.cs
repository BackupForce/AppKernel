using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.GetById;

/// <summary>
/// 取得期數詳細資訊查詢。
/// </summary>
public sealed record GetDrawByIdQuery(Guid DrawId) : IQuery<DrawDetailDto>;
