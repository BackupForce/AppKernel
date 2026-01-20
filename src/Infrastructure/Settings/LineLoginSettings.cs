using Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;

namespace Infrastructure.Settings;

internal sealed class LineLoginSettings(IOptions<LineLoginOptions> options) : ILineLoginSettings
{
    private readonly LineLoginOptions _options = options.Value;

    public string EmailDomain => _options.EmailDomain;
}
