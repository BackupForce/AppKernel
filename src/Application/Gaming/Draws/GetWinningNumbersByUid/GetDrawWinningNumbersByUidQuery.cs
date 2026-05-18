using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetWinningNumbersByUid;

public sealed record GetDrawWinningNumbersByUidQuery(Guid id)
    : IQuery<DrawWinningNumbersDto>;
