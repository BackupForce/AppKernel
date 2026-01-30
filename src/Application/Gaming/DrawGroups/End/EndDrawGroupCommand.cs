using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.End;

/// <summary>
/// 結束活動命令。
/// </summary>
public sealed record EndDrawGroupCommand(Guid TenantId, Guid DrawGroupId) : ICommand;
