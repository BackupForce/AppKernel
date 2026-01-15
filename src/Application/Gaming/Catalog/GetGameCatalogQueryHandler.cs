using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using SharedKernel;

namespace Application.Gaming.Catalog;

internal sealed class GetGameCatalogQueryHandler : IQueryHandler<GetGameCatalogQuery, IReadOnlyCollection<GameCatalogDto>>
{
    public Task<Result<IReadOnlyCollection<GameCatalogDto>>> Handle(
        GetGameCatalogQuery request,
        CancellationToken cancellationToken)
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Dictionary<string, HashSet<string>> mapping = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (GameCode gameCode in new[] { GameCodes.Lottery539 })
        {
            IReadOnlyCollection<PlayTypeCode> plays = registry.GetAllowedPlayTypes(gameCode);
            HashSet<string> playCodes = new HashSet<string>(plays.Select(play => play.Value), StringComparer.OrdinalIgnoreCase);
            mapping[gameCode.Value] = playCodes;
        }

        IReadOnlyCollection<GameCatalogDto> result = mapping
            .Select(pair => new GameCatalogDto(pair.Key, pair.Value.OrderBy(value => value).ToList()))
            .OrderBy(item => item.GameCode)
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyCollection<GameCatalogDto>>(result));
    }
}
