namespace Application.Gaming.Dtos;

public sealed record AvailableTicketsResponse(IReadOnlyList<AvailableTicketItemDto> Items);
