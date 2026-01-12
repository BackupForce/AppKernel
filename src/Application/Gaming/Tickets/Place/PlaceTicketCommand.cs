using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Place;

public sealed record PlaceTicketCommand(
    Guid DrawId,
    IReadOnlyCollection<IReadOnlyCollection<int>> Lines) : ICommand<Guid>;
