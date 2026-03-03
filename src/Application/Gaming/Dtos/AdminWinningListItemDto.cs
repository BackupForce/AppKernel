using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

public sealed class AdminWinningListItemDto
{
    public Guid WinningId { get; set; }
    public Guid TicketId { get; set; }
    public Guid DrawId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string MemberDisplayName { get; set; } = string.Empty;
    public string DrawCode { get; set; } = string.Empty;
    public string? PrizeName { get; set; }
    public DateTime DrawDateUtc { get; set; }
    public decimal PayoutAmount { get; set; }
    public TicketDrawParticipationStatus Status { get; set; }
    public DateTime? RedeemedAtUtc { get; set; }
    public Guid? RedeemedByUserId { get; set; }
    public string? RedeemedByUserName { get; set; }
}
