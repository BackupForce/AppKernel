using Application.Abstractions.Messaging;
using Domain.Gaming.TicketTemplates;

namespace Application.Gaming.TicketTemplates.Create;

/// <summary>
/// 建立票種模板命令。
/// </summary>
public sealed record CreateTicketTemplateCommand(
    string Code,
    string Name,
    TicketTemplateType Type,
    decimal Price,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int MaxLinesPerTicket) : ICommand<Guid>;
