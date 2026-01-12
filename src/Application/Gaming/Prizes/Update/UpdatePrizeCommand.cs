using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Update;

/// <summary>
/// 更新獎品命令。
/// </summary>
public sealed record UpdatePrizeCommand(
    Guid PrizeId,
    string Name,
    string? Description,
    decimal Cost) : ICommand;
