namespace Application.Gaming.Dtos;

/// <summary>
/// 期數詳細資料，包含公平性驗證所需的 proof 欄位。
/// </summary>
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
