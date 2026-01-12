using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Deactivate;

/// <summary>
/// 停用獎品命令。
/// </summary>
public sealed record DeactivatePrizeCommand(Guid PrizeId) : ICommand;
