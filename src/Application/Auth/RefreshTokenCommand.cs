using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? UserAgent,
    string? Ip) : ICommand<RefreshTokenResponse>;
