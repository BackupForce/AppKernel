using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LineLoginCommand(
    string AccessToken,
    string? DeviceId,
    string? UserAgent,
    string? Ip) : ICommand<LineLoginResponse>;
