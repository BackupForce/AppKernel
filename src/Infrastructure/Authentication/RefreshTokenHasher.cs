using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

internal sealed class RefreshTokenHasher(IOptions<AuthTokenOptions> options) : IRefreshTokenHasher
{
    private readonly AuthTokenOptions _options = options.Value;

    public string Hash(string tokenPlain)
    {
        string input = string.Concat(tokenPlain, "::", _options.RefreshTokenPepper);
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string tokenPlain, string tokenHash)
    {
        string computed = Hash(tokenPlain);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(tokenHash));
    }
}
