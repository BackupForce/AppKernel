namespace Application.Members.Dtos;

public sealed record MemberListItemDto(
    Guid Id,
    Guid? UserId,
    string MemberNo,
    string DisplayName,
    short Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ProfileRealName,
    short? ProfileGender,
    string? ProfilePhoneNumber,
    bool? ProfilePhoneVerified,
    DateTime? ProfileUpdatedAtUtc);
