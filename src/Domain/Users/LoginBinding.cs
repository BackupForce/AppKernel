using SharedKernel;

namespace Domain.Users;

public sealed class LoginBinding : Entity
{
    private LoginBinding(
        Guid id,
        Guid userId,
        Guid? tenantId,
        LoginProvider provider,
        string providerKey,
        string normalizedProviderKey,
        DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        Provider = provider;
        ProviderKey = providerKey;
        NormalizedProviderKey = normalizedProviderKey;
        CreatedAtUtc = createdAtUtc;
    }

    private LoginBinding()
    {
    }

    public Guid UserId { get; private set; }

    public Guid? TenantId { get; private set; }

    public LoginProvider Provider { get; private set; }

    public string ProviderKey { get; private set; } = string.Empty;

    public string NormalizedProviderKey { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public User? User { get; set; }

    public static Result<LoginBinding> Create(
        LoginProvider provider,
        string providerKey,
        Guid userId,
        Guid? tenantId,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return Result.Failure<LoginBinding>(UserErrors.LoginProviderKeyRequired);
        }

        string normalized = Normalize(provider, providerKey);
        return Result.Success(new LoginBinding(
            Guid.NewGuid(),
            userId,
            tenantId,
            provider,
            providerKey.Trim(),
            normalized,
            utcNow));
    }

    public static string Normalize(LoginProvider provider, string providerKey)
    {
        string trimmed = providerKey.Trim();

        return provider switch
        {
            LoginProvider.Line => trimmed,
            LoginProvider.Email => trimmed.ToUpperInvariant(),
            _ => trimmed
        };
    }

    public void UpdateTenantId(Guid? tenantId)
    {
        TenantId = tenantId;
    }
}
