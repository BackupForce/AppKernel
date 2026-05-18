using System.Security.Cryptography;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSize = 64;

    public string GenerateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(TokenSize);
        string token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return token;
    }
}
