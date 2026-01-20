using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand;
