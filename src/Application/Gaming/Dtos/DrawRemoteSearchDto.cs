namespace Application.Gaming.Dtos;

/// <summary>
/// 期數遠端搜尋結果，用於下拉或搜尋框。
/// </summary>
public sealed record DrawRemoteSearchDto(
    Guid Id,
    string Name);
