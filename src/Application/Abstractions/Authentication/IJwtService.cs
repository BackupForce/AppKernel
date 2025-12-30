using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Authentication;
public interface IJwtService
{
    string GenerateToken(Guid userId, string userName, IEnumerable<string> roles, IEnumerable<Guid> nodeIds, IEnumerable<string> permissions);
    JwtPayloadDto? ValidateToken(string token);
}

