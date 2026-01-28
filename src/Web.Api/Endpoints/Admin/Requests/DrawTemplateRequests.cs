namespace Web.Api.Endpoints.Admin.Requests;

public sealed record CreateDrawTemplateRequest(
    string GameCode,
    string Name,
    bool IsActive,
    IReadOnlyCollection<DrawTemplatePlayTypeRequest> PlayTypes,
    IReadOnlyCollection<Guid> AllowedTicketTemplateIds);

public sealed record UpdateDrawTemplateRequest(
    string Name,
    IReadOnlyCollection<DrawTemplatePlayTypeRequest> PlayTypes,
    IReadOnlyCollection<Guid> AllowedTicketTemplateIds);

public sealed record DrawTemplatePlayTypeRequest(
    string PlayTypeCode,
    IReadOnlyCollection<DrawTemplatePrizeTierRequest> PrizeTiers);

public sealed record DrawTemplatePrizeTierRequest(
    string Tier,
    DrawTemplatePrizeOptionRequest Option);

public sealed record DrawTemplatePrizeOptionRequest(
    Guid? PrizeId,
    string Name,
    decimal Cost,
    int? RedeemValidDays,
    string? Description);

public sealed record GetDrawTemplatesRequest(
    string? GameCode,
    bool? IsActive);
