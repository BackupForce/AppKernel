using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Activate;

/// <summary>
/// 啟用獎品命令。
/// </summary>
public sealed record ActivatePrizeCommand(Guid PrizeId) : ICommand;
