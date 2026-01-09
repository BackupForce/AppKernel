namespace Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithGroupsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetTenantUserByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);
    Task<User?> GetMemberByNormalizedLineUserIdAsync(Guid tenantId, string normalizedLineUserId, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(Email email);

    void Insert(User user);
}
