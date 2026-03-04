using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

public sealed class RedeemedWinningListItemDto
{
    public Guid WinningId { get; set; }
    public Guid TicketId { get; set; }
    public Guid MemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberDisplayName { get; set; } = string.Empty;
    public string? MemberPhoneNumber { get; set; }
    public string DrawCode { get; set; } = string.Empty;
    public string? PrizeName { get; set; }
    public decimal PrizeAmount { get; set; }
    public TicketDrawParticipationStatus Status { get; set; }
    public DateTime? RedeemedAtUtc { get; set; }
    public string? RedeemedBy { get; set; }
}
