using Application.Abstractions.Messaging;

namespace Application.Auth;

public sealed record GetSessionsQuery : IQuery<IReadOnlyCollection<AuthSessionDto>>;
