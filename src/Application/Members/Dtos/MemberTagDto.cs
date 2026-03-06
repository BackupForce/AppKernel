namespace Application.Members.Dtos;

public sealed record MemberTagDto(
    Guid Id,
    string TagCode,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
