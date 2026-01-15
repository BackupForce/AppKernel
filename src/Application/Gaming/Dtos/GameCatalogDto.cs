namespace Application.Gaming.Dtos;

public sealed record GameCatalogDto(
    string GameCode,
    IReadOnlyCollection<string> PlayTypeCodes);
