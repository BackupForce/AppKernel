using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Submit;

public sealed record SubmitTicketNumbersCommand(Guid TicketId, IReadOnlyCollection<int> Numbers) : ICommand;
