using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Execute;

public sealed record ExecuteDrawCommand(Guid DrawId) : ICommand;
