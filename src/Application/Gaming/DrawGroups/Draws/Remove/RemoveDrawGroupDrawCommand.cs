using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Draws.Remove;

/// <summary>
/// 移除期數群組期數命令。
/// </summary>
public sealed record RemoveDrawGroupDrawCommand(
    Guid TenantId,
    Guid DrawGroupId,
    Guid DrawId) : ICommand;
