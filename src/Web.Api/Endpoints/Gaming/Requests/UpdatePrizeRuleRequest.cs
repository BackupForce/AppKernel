namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record UpdatePrizeRuleRequest(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo);
