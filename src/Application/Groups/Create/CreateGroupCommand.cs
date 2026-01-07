using Application.Abstractions.Messaging;

namespace Application.Groups.Create;

public sealed record CreateGroupCommand(string Name, string ExternalKey) : ICommand<Guid>;
