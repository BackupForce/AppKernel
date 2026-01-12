namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 更新期數獎項對應清單請求。
/// </summary>
public sealed record UpdateDrawPrizeMappingsRequest(IReadOnlyCollection<DrawPrizeMappingItemRequest> Mappings);

/// <summary>
/// 期數獎項對應項目請求。
/// </summary>
public sealed record DrawPrizeMappingItemRequest(int MatchCount, IReadOnlyCollection<Guid> PrizeIds);
