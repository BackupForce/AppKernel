using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Reopen;

/// <summary>
/// 解封期數命令。
/// </summary>
public sealed record ReopenDrawCommand(Guid DrawId) : ICommand;
