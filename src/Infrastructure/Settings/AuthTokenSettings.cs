using Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;

namespace Infrastructure.Settings;

internal sealed class AuthTokenSettings(IOptions<AuthTokenOptions> options) : IAuthTokenSettings
{
    private readonly AuthTokenOptions _options = options.Value;

    public int RefreshTokenTtlDays => _options.RefreshTokenTtlDays;
}
