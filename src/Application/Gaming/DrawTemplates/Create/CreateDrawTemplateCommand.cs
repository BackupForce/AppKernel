using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawTemplates.Create;

/// <summary>
/// 建立期數模板命令。
/// </summary>
public sealed record CreateDrawTemplateCommand(
    string GameCode,
    string Name,
    bool IsActive,
    IReadOnlyCollection<DrawTemplatePlayTypeInput> PlayTypes,
    IReadOnlyCollection<Guid> AllowedTicketTemplateIds) : ICommand<Guid>;
