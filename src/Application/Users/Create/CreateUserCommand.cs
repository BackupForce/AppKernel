using Application.Abstractions.Messaging;

namespace Application.Users.Create;

public sealed record CreateUserCommand(string Email, string Name, string Password,  bool HasPublicProfile)
    : ICommand<Guid>;
