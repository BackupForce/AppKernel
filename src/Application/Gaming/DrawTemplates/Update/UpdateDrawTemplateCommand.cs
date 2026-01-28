using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawTemplates.Update;

/// <summary>
/// 更新期數模板命令。
/// </summary>
public sealed record UpdateDrawTemplateCommand(
    Guid TemplateId,
    string Name,
    IReadOnlyCollection<DrawTemplatePlayTypeInput> PlayTypes,
    IReadOnlyCollection<Guid> AllowedTicketTemplateIds) : ICommand;
