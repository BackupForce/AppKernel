using Application.Abstractions.Messaging;

namespace Application.Users.Activation;

public sealed record EnableUserCommand(Guid UserId) : ICommand;
