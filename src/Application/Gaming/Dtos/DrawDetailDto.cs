namespace Application.Gaming.Dtos;

/// <summary>
/// 期數詳細資料，包含公平性驗證所需的 proof 欄位。
/// </summary>
public sealed record DrawDetailDto(
    Guid Id,
    string GameCode,
    DateTime SalesStartAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    string Status,
    bool IsManuallyClosed,
    DateTime? ManualCloseAt,
    string? ManualCloseReason,
    int? RedeemValidDays,
    string? WinningNumbers,
    string? ServerSeedHash,
    string? ServerSeed,
    string? Algorithm,
    string? DerivedInput);
