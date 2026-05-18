using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Settle;

/// <summary>
/// 結算期數命令，產生得獎記錄。
/// </summary>
public sealed record SettleDrawCommand(Guid DrawId) : ICommand;
