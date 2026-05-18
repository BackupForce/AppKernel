namespace Application.Members.Dtos;

public sealed record MemberPointBalanceDto(
    Guid MemberId,
    long Balance,
    DateTime UpdatedAt);
