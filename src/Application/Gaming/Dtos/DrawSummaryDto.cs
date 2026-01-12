namespace Application.Gaming.Dtos;

public sealed record DrawSummaryDto(
    Guid Id,
    DateTime SalesOpenAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    string Status);
