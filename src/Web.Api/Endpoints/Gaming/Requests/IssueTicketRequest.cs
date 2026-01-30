using System.Text.Json.Serialization;

namespace Web.Api.Endpoints.Gaming.Requests;

public sealed class IssueTicketRequest
{
    public Guid MemberId { get; init; }

    public Guid? DrawGroupId { get; init; }

    [JsonPropertyName("campaignId")]
    public Guid? CampaignId { get; init; }

    public Guid? TicketTemplateId { get; init; }

    public string? IssuedReason { get; init; }

    public Guid ResolveDrawGroupId() => DrawGroupId ?? CampaignId ?? Guid.Empty;
}
