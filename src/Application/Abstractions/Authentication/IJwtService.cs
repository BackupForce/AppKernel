using System;
using System.Collections.Generic;
using Domain.Users;

namespace Application.Abstractions.Authentication;
public interface IJwtService
{
    string GenerateToken(
        Guid userId,
        string userName,
        UserType userType,
        Guid? tenantId,
        IEnumerable<string> roles,
        IEnumerable<Guid> nodeIds,
        IEnumerable<string> permissions);
    JwtPayloadDto? ValidateToken(string token);
}
