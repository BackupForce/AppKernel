using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketTemplates.Activate;

/// <summary>
/// 啟用票種模板命令。
/// </summary>
public sealed record ActivateTicketTemplateCommand(Guid TemplateId) : ICommand;
