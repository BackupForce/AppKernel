namespace Application.Gaming.Dtos;

/// <summary>
/// 獎品資料傳輸物件。
/// </summary>
public sealed record PrizeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Cost,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
