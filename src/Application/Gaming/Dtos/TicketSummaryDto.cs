using System.Text.Json.Serialization;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

/// <summary>
/// 票券摘要資料，提供會員查詢。
/// </summary>
public sealed partial record TicketSummaryDto(
    Guid TicketId,
    Guid? DrawGroupId,
    string GameCode,
    string? PlayTypeCode,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    IReadOnlyCollection<TicketLineSummaryDto> Lines,
    IReadOnlyCollection<TicketDrawSummaryDto> Draws);

public sealed partial record TicketSummaryDto
{
    [JsonPropertyName("campaignId")]
    public Guid? CampaignId => DrawGroupId;
}
