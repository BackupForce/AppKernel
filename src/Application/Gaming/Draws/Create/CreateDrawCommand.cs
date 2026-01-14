using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Create;

/// <summary>
/// 建立 539 期數的命令。
/// </summary>
public sealed record CreateDrawCommand(
    string GameCode,
    IReadOnlyCollection<string> EnabledPlayTypes,
    DateTime SalesStartAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    int? RedeemValidDays) : ICommand<Guid>;
