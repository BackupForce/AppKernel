using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.AllowedTicketTemplates.Update;

/// <summary>
/// 更新期數允許的票種模板清單（覆寫語意）。
/// </summary>
public sealed record UpdateDrawAllowedTicketTemplatesCommand(
    Guid DrawId,
    IReadOnlyCollection<Guid> TemplateIds) : ICommand;
