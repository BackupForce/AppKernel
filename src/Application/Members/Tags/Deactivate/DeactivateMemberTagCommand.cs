using Application.Abstractions.Messaging;

namespace Application.Members.Tags.Deactivate;

public sealed record DeactivateMemberTagCommand(
    Guid TenantId,
    Guid TagId) : ICommand;
