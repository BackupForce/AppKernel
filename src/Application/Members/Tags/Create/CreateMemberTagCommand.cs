using Application.Abstractions.Messaging;

namespace Application.Members.Tags.Create;

public sealed record CreateMemberTagCommand(
    Guid TenantId,
    string TagCode,
    string DisplayName) : ICommand<Guid>;
