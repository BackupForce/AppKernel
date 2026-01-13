using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.PrizeMappings.Update;

/// <summary>
/// 更新期數獎項對應設定（覆寫語意）。
/// </summary>
public sealed record UpdateDrawPrizeMappingsCommand(
    Guid DrawId,
    IReadOnlyCollection<DrawPrizeMappingInput> Mappings) : ICommand;

/// <summary>
/// 期數獎項對應輸入資料。
/// </summary>
public sealed record DrawPrizeMappingInput(int MatchCount, IReadOnlyCollection<Guid> PrizeIds);
