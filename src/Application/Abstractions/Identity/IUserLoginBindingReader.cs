using Domain.Users;

namespace Application.Abstractions.Identity;

public interface IUserLoginBindingReader
{
    Task<User?> FindUserByLoginAsync(
        Guid? tenantId,
        LoginProvider provider,
        string normalizedKey,
        CancellationToken cancellationToken = default);
}
