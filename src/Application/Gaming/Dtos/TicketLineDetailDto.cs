namespace Application.Gaming.Dtos;

/// <summary>
/// 票券注數明細（含玩法）。
/// </summary>
public sealed record TicketLineDetailDto(
    int LineIndex,
    string PlayTypeCode,
    string Numbers);
