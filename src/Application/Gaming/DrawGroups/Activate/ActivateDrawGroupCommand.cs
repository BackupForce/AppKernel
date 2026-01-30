using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Activate;

/// <summary>
/// 啟用活動命令。
/// </summary>
public sealed record ActivateDrawGroupCommand(Guid TenantId, Guid DrawGroupId) : ICommand;
