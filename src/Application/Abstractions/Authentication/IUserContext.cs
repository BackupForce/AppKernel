using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    UserType UserType { get; }

    Guid? TenantId { get; }
}
