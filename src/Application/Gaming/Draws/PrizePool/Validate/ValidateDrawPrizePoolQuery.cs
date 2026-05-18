using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.PrizePool.Validate;

public sealed record ValidateDrawPrizePoolQuery(Guid DrawId) : IQuery<DrawPrizePoolValidationDto>;
