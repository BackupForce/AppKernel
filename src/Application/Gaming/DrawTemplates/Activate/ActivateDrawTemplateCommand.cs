using Application.Abstractions.Messaging;

namespace Application.Gaming.DrawTemplates.Activate;

public sealed record ActivateDrawTemplateCommand(Guid TemplateId) : ICommand;
