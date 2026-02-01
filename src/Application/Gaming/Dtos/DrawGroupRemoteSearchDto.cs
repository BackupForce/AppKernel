namespace Application.Gaming.Dtos;

/// <summary>
/// 期數群組遠端搜尋結果，用於下拉或搜尋框。
/// </summary>
public sealed record DrawGroupRemoteSearchDto(
    Guid Id,
    string Name);
