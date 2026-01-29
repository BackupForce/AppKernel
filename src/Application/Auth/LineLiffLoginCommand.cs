using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LineLiffLoginCommand(
    string AccessToken,
    string? DisplayName,
    Uri? PictureUrl,
    string? Email,
    string? DeviceId,
    string? UserAgent,
    string? Ip) : ICommand<LineLoginResponse>;
