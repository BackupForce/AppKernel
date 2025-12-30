namespace Application.Members.Dtos;

public sealed record MemberPointLedgerDto(
    Guid Id,
    Guid MemberId,
    short Type,
    long Amount,
    long BeforeBalance,
    long AfterBalance,
    string? ReferenceType,
    string? ReferenceId,
    Guid? OperatorUserId,
    string? Remark,
    DateTime CreatedAt);
