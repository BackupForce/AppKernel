namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 建立中獎規則的 API 請求資料。
/// </summary>
public sealed record CreatePrizeRuleRequest(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo);
