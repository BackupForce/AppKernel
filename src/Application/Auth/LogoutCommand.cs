using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
