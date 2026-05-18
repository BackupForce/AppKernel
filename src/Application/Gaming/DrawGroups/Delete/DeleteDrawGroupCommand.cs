using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawGroups.Delete;

/// <summary>
/// 刪除活動命令。
/// </summary>
public sealed record DeleteDrawGroupCommand(Guid TenantId, Guid DrawGroupId) : ICommand;
