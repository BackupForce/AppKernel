namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record CreatePrizeRuleRequest(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo);
