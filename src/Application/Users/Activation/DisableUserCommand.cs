using Application.Abstractions.Messaging;

namespace Application.Users.Activation;

public sealed record DisableUserCommand(Guid UserId) : ICommand;
