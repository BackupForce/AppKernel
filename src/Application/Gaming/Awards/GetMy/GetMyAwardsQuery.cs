using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Awards.GetMy;

public sealed record GetMyAwardsQuery(string? Status) : IQuery<IReadOnlyCollection<PrizeAwardDto>>;
