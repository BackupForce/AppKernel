using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetWinningNumbersByUid;

public sealed record GetDrawWinningNumbersByUidQuery(string Uid)
    : IQuery<DrawWinningNumbersDto>;
