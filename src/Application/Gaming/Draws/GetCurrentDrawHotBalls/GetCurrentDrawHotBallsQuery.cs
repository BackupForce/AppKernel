using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetCurrentDrawHotBalls;

public sealed record GetCurrentDrawHotBallsQuery : IQuery<IReadOnlyCollection<HotBallDto>>;
