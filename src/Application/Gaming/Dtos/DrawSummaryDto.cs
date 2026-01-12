namespace Application.Gaming.Dtos;

/// <summary>
/// 期數摘要資料，用於列表顯示。
/// </summary>
public sealed record DrawSummaryDto(
    Guid Id,
    DateTime SalesStartAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    string Status);
