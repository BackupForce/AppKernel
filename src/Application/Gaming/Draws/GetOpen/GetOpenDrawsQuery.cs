using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.GetOpen;

public sealed record GetOpenDrawsQuery : IQuery<IReadOnlyCollection<DrawSummaryDto>>;
