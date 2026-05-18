namespace Application.Gaming.DrawTemplates;

public sealed record DrawTemplatePrizeOptionInput(
    Guid? PrizeId,
    string Name,
    decimal Cost,
    decimal PayoutAmount,
    int? RedeemValidDays,
    string? Description);

public sealed record DrawTemplatePrizeTierInput(
    string Tier,
    DrawTemplatePrizeOptionInput Option);

public sealed record DrawTemplatePlayTypeInput(
    string PlayTypeCode,
    IReadOnlyCollection<DrawTemplatePrizeTierInput> PrizeTiers);
