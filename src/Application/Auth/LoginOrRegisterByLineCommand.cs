using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record LoginOrRegisterByLineCommand(string LineUserId, string LineUserName)
    : ICommand<LineLoginResultDto>;
