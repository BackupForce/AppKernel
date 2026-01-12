using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Prizes.GetList;

public sealed record GetPrizeListQuery : IQuery<IReadOnlyCollection<PrizeDto>>;
