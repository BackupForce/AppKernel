namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 手動封盤請求資料。
/// </summary>
public sealed record CloseDrawManuallyRequest(string? Reason);
