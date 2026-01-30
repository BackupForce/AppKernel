using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.DrawGroups.GetById;

/// <summary>
/// 取得活動詳細資訊查詢。
/// </summary>
public sealed record GetDrawGroupByIdQuery(Guid TenantId, Guid DrawGroupId) : IQuery<DrawGroupDetailDto>;
