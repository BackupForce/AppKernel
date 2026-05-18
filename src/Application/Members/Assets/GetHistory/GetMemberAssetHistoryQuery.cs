using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Application.Members.Dtos;

namespace Application.Members.Assets.GetHistory;

public sealed record GetMemberAssetHistoryQuery(
    Guid MemberId,
    string AssetCode,
    DateTime? StartDate,
    DateTime? EndDate,
    short? Type,
    string? ReferenceType,
    string? ReferenceId,
    int Page,
    int PageSize) : IQuery<PagedResult<MemberAssetLedgerDto>>;
