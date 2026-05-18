namespace Application.Abstractions.Gaming;

public sealed record TenantEntitlementsDto(
    IReadOnlyCollection<string> EnabledGameCodes,
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> EnabledPlayTypesByGame);
