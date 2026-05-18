using Application.Gaming.Draws.PrizePool;

namespace Application.Gaming.Dtos;

public sealed record DrawTemplateSummaryDto(
    Guid Id,
    string GameCode,
    string Name,
    bool IsActive,
    bool IsLocked,
    int Version,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record DrawTemplateDetailDto(
    Guid Id,
    string GameCode,
    string Name,
    bool IsActive,
    bool IsLocked,
    int Version,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<DrawTemplatePlayTypeDto> PlayTypes,
    IReadOnlyCollection<Guid> AllowedTicketTemplateIds);

public sealed record DrawTemplatePlayTypeDto(
    string PlayTypeCode,
    IReadOnlyCollection<DrawTemplatePrizeTierDto> PrizeTiers);

public sealed record DrawTemplatePrizeTierDto(
    string Tier,
    PrizeOptionDto Option);
