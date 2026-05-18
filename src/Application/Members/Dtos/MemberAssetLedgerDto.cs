namespace Application.Members.Dtos;

public sealed record MemberAssetLedgerDto(
    Guid Id,
    Guid MemberId,
    string AssetCode,
    short Type,
    decimal Amount,
    decimal BeforeBalance,
    decimal AfterBalance,
    string? ReferenceType,
    string? ReferenceId,
    Guid? OperatorUserId,
    string? Remark,
    DateTime CreatedAt);
