using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketTemplates.Deactivate;

/// <summary>
/// 停用票種模板命令。
/// </summary>
public sealed record DeactivateTicketTemplateCommand(Guid TemplateId) : ICommand;
