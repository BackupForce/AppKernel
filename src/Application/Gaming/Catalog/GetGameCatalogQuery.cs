using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Catalog;

public sealed record GetGameCatalogQuery : IQuery<IReadOnlyCollection<GameCatalogDto>>;
