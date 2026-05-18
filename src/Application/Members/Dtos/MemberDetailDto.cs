using Application.Members.Profiles;

namespace Application.Members.Dtos;

public sealed record MemberDetailDto(
    Guid Id,
    Guid? UserId,
    string MemberNo,
    string DisplayName,
    short Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MemberProfileDto? Profile,
    IReadOnlyList<LoginBindingDto> LoginBindings);
