namespace Application.Gaming.Dtos;

/// <summary>
/// 可售票期數下拉選項。
/// </summary>
public sealed record DrawSellingOptionDto(
    Guid Value,
    string Label,
    DateTime SalesCloseAtUtc,
    DateTime DrawAtUtc);
