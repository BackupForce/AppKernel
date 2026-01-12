using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Create;

public sealed record CreateDrawCommand(
    DateTime SalesOpenAt,
    DateTime SalesCloseAt,
    DateTime DrawAt) : ICommand<Guid>;
