using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Update;

/// <summary>
/// 更新期數群組基本資料命令。
/// </summary>
public sealed record UpdateDrawGroupCommand(
    Guid TenantId,
    Guid DrawGroupId,
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc) : ICommand;
