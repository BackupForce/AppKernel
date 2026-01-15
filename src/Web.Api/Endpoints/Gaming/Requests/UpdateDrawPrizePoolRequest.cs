namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 更新期數獎項配置請求。
/// </summary>
public sealed record UpdateDrawPrizePoolRequest(IReadOnlyCollection<UpdateDrawPrizePoolItemRequest> Items);

/// <summary>
/// 期數獎項配置項目請求。
/// </summary>
public sealed record UpdateDrawPrizePoolItemRequest(
    string PlayTypeCode,
    string Tier,
    PrizeOptionRequest Option);

/// <summary>
/// 獎項快照請求。
/// </summary>
public sealed record PrizeOptionRequest(
    Guid? PrizeId,
    string Name,
    decimal Cost,
    int? RedeemValidDays,
    string? Description);
