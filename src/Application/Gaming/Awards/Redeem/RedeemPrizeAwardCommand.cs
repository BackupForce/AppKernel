using Application.Abstractions.Messaging;

namespace Application.Gaming.Awards.Redeem;

public sealed record RedeemPrizeAwardCommand(Guid AwardId, string? Note) : ICommand<Guid>;
