namespace Application.Gaming.Dtos;

/// <summary>
/// 期數群組綁定期數資料。
/// </summary>
public sealed record DrawGroupDrawDto(
    Guid DrawId,
    DateTime CreatedAtUtc);
