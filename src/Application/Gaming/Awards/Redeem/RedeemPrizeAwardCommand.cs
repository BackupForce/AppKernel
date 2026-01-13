using Application.Abstractions.Messaging;

namespace Application.Gaming.Awards.Redeem;

/// <summary>
/// 兌換得獎獎品命令。
/// </summary>
public sealed record RedeemPrizeAwardCommand(Guid AwardId, Guid PrizeId, string? Note) : ICommand<Guid>;
