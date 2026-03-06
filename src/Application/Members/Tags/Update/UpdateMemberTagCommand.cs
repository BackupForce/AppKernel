using Application.Abstractions.Messaging;

namespace Application.Members.Tags.Update;

public sealed record UpdateMemberTagCommand(
    Guid TenantId,
    Guid TagId,
    string DisplayName) : ICommand;
