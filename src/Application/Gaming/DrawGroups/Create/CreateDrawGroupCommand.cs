using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Create;

/// <summary>
/// 建立期數群組的命令。
/// </summary>
public sealed record CreateDrawGroupCommand(
    Guid TenantId,
    string GameCode,
    string PlayTypeCode,
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc) : ICommand<Guid>;
