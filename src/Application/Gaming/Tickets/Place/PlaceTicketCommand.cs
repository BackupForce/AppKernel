using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Place;

/// <summary>
/// 下注命令，包含期數與投注號碼。
/// </summary>
public sealed record PlaceTicketCommand(
    Guid DrawId,
    string PlayTypeCode,
    Guid TemplateId,
    IReadOnlyCollection<IReadOnlyCollection<int>> Lines) : ICommand<Guid>;
