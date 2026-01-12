using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Create;

/// <summary>
/// 建立獎品命令。
/// </summary>
public sealed record CreatePrizeCommand(
    string Name,
    string? Description,
    decimal Cost) : ICommand<Guid>;
