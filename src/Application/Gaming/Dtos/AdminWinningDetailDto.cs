using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

public sealed class AdminWinningDetailDto
{
    public Guid WinningId { get; set; }
    public Guid TenantId { get; set; }
    public Guid TicketId { get; set; }
    public Guid DrawId { get; set; }
    public DateTime DrawDateUtc { get; set; }
    public string DrawCode { get; set; } = string.Empty;
    public decimal PayoutAmount { get; set; }
    public string PrizeTier { get; set; } = string.Empty;
    public TicketDrawParticipationStatus Status { get; set; }
    public DateTime SettledAtUtc { get; set; }
    public DateTime? RedeemedAtUtc { get; set; }
    public Guid? RedeemedByUserId { get; set; }
    public string? RedeemedByUserName { get; set; }
}
