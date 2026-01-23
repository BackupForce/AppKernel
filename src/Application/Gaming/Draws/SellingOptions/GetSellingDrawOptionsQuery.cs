using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.SellingOptions;

/// <summary>
/// 取得可售票期數下拉選項。
/// </summary>
public sealed record GetSellingDrawOptionsQuery(
    string? GameCode,
    string? PlayTypeCode,
    int? Take) : IQuery<IReadOnlyList<DrawSellingOptionDto>>;
