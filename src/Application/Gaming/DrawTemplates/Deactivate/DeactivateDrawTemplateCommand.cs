using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawTemplates.Deactivate;

public sealed record DeactivateDrawTemplateCommand(Guid TemplateId) : ICommand;
