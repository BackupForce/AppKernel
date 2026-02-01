using System.Text.Json.Serialization;

namespace Web.Api.Endpoints.Gaming.Requests;

public sealed class IssueTicketRequest
{
    public Guid MemberId { get; init; }

    public Guid? DrawGroupId { get; init; }

    public Guid? TicketTemplateId { get; init; }

    public string? IssuedReason { get; init; }
    public Guid ResolveDrawGroupId() => DrawGroupId ?? Guid.Empty;
}
