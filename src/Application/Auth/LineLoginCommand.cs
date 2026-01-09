using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LineLoginCommand(string AccessToken) : ICommand<LineLoginResponse>;
