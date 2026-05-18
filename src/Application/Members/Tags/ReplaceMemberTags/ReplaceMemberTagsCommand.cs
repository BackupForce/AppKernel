using Application.Abstractions.Messaging;

namespace Application.Members.Tags.ReplaceMemberTags;

public sealed record ReplaceMemberTagsCommand(
    Guid TenantId,
    Guid MemberId,
    IReadOnlyCollection<Guid> TagIds) : ICommand;
