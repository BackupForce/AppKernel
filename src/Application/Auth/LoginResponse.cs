using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Auth;
public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime Expiration { get; init; }
}

