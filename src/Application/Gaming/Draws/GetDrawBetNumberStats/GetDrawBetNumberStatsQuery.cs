using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetDrawBetNumberStats;

public sealed record GetDrawBetNumberStatsQuery(Guid DrawId) : IQuery<IReadOnlyCollection<DrawBetNumberStatDto>>;
