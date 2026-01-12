namespace Application.Gaming.Dtos;

/// <summary>
/// 兌獎選項快照資料。
/// </summary>
public sealed record PrizeAwardOptionDto(
    Guid PrizeId,
    string PrizeName,
    decimal PrizeCost);
