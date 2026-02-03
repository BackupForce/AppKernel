namespace Web.Api.Endpoints.Frontends.Requests;

/// <summary>
/// 下注請求資料，包含每一注的號碼。
/// </summary>
public sealed record PlaceTicketRequest(
    Guid DrawId,
    string PlayTypeCode,
    Guid TemplateId,
    IReadOnlyCollection<IReadOnlyCollection<int>> Lines);
