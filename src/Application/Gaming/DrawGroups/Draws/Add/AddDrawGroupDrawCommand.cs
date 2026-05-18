using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Draws.Add;

/// <summary>
/// 綁定期數群組期數命令。
/// </summary>
public sealed record AddDrawGroupDrawCommand(
    Guid TenantId,
    Guid DrawGroupId,
    Guid DrawId) : ICommand;
