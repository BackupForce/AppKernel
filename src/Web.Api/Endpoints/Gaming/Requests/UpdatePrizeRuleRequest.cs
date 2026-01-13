namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 更新中獎規則的 API 請求資料。
/// </summary>
public sealed record UpdatePrizeRuleRequest(
    int MatchCount,
    Guid PrizeId,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    int? RedeemValidDays);
