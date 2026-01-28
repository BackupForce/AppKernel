using System.Globalization;
using Application.Abstractions.Gaming;
using Domain.Gaming.Catalog;

namespace Infrastructure.Gaming;

internal sealed class DrawCodeGenerator(IDrawSequenceService drawSequenceService)
: IDrawCodeGenerator
{
    public async Task<string> IssueDrawCodeAsync(
    Guid tenantId,
    GameCode gameCode,
    DateTime drawAtUtc,
    DateTime nowUtc,
    CancellationToken cancellationToken)
    {
        long sequence = await drawSequenceService.IssueNextAsync(
        tenantId,
        gameCode,
        nowUtc,
        cancellationToken);


        return string.Format(
        CultureInfo.InvariantCulture,
        "{0}-{1:yyMM}{2}",
        gameCode.Value,
        drawAtUtc,
        sequence.ToString("D3", CultureInfo.InvariantCulture));
    }
}
