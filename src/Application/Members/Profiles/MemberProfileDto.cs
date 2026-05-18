using Domain.Members;

namespace Application.Members.Profiles;

public sealed record MemberProfileDto(
    Guid MemberId,
    string? RealName,
    Gender Gender,
    string? PhoneNumber,
    bool PhoneVerified,
    DateTime UpdatedAtUtc);
