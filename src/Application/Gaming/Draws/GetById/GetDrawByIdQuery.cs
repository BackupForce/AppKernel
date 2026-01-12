using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.GetById;

public sealed record GetDrawByIdQuery(Guid DrawId) : IQuery<DrawDetailDto>;
