using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LineLiffLoginCommand(
    string AccessToken,
    string? DisplayName,
    string? DeviceId,
    string? UserAgent,
    string? Ip) : ICommand<LineLoginResponse>;
