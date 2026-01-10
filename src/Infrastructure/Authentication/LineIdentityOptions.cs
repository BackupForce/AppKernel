using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Authentication;
public sealed class LineIdentityOptions
{
    public Uri? VerifyAccessTokenEndpoint { get; init; }
    public Uri? ProfileEndpoint { get; init; }

    public Uri? VerifyIdTokenEndpoint { get; init; }
    public string? ChannelId { get; init; }
}
