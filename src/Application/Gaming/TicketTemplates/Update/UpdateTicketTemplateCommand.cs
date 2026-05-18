using Application.Abstractions.Messaging;
using Domain.Gaming.TicketTemplates;

namespace Application.Gaming.TicketTemplates.Update;

/// <summary>
/// 更新票種模板命令。
/// </summary>
public sealed record UpdateTicketTemplateCommand(
    Guid TemplateId,
    string Code,
    string Name,
    TicketTemplateType Type,
    decimal Price,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int MaxLinesPerTicket) : ICommand;
