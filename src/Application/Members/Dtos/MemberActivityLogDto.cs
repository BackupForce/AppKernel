namespace Application.Members.Dtos;

public sealed record MemberActivityLogDto(
    Guid Id,
    Guid MemberId,
    string Action,
    string? Ip,
    string? UserAgent,
    Guid? OperatorUserId,
    string? Payload,
    DateTime CreatedAt);
