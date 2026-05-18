using Application.Abstractions.Messaging;
using Domain.Members;

namespace Application.Members.Profiles;

public sealed record UpsertMemberProfileCommand(
    Guid MemberId,
    string? RealName,
    Gender Gender,
    string? PhoneNumber,
    bool PhoneVerified) : ICommand<MemberProfileDto>;
