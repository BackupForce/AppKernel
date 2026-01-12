using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.ManualClose;

/// <summary>
/// 手動封盤命令。
/// </summary>
public sealed record CloseDrawManuallyCommand(Guid DrawId, string? Reason) : ICommand;
