using Application.Abstractions.Messaging;

namespace Application.Members.Tags.ActivateMemberTag;

public sealed record ActivateMemberTagCommand(
    Guid TenantId,
    Guid MemberTagId) : ICommand;
