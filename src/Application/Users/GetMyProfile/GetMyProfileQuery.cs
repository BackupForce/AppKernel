using Application.Abstractions.Messaging;

namespace Application.Users.GetMyProfile;

public sealed record GetMyProfileQuery : IQuery<MyProfileDto>;
