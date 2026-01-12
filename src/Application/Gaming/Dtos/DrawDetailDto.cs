namespace Application.Gaming.Dtos;

public sealed record DrawDetailDto(
    Guid Id,
    DateTime SalesOpenAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    string Status,
    string? WinningNumbers,
    string? ServerSeedHash,
    string? ServerSeed,
    string? Algorithm,
    string? DerivedInput);
