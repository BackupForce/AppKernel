using Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

internal sealed class UserTypeRequirement : IAuthorizationRequirement
{
    public UserTypeRequirement(IReadOnlyCollection<UserType> allowedTypes, bool enforceTenantMatch)
    {
        AllowedTypes = allowedTypes;
        EnforceTenantMatch = enforceTenantMatch;
    }

    public IReadOnlyCollection<UserType> AllowedTypes { get; }

    public bool EnforceTenantMatch { get; }
}
