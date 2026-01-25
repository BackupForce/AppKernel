using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Submit;

public sealed record SubmitTicketNumbersCommand(
    Guid TicketId,
    string PlayTypeCode,
    IReadOnlyCollection<int> Numbers) : ICommand;
