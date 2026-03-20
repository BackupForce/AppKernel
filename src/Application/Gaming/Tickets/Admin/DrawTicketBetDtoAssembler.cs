using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin;

internal static class DrawTicketBetDtoAssembler
{
    public static IReadOnlyList<DrawTicketBetDto> Assemble(IEnumerable<TicketBetRow> rows)
    {
        Dictionary<Guid, DrawTicketBetDto> ticketMap = new();
        Dictionary<Guid, List<TicketLineDetailDto>> lineMap = new();
        List<Guid> ticketOrder = new();

        foreach (TicketBetRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new DrawTicketBetDto(
                    row.TicketId,
                    row.MemberId,
                    row.MemberNo,
                    row.DisplayName,
                    row.GameCode,
                    row.SubmissionStatus,
                    row.IssuedAtUtc,
                    row.SubmittedAtUtc,
                    row.ParticipationStatus,
                    Array.Empty<TicketLineDetailDto>());
                lineMap[row.TicketId] = new List<TicketLineDetailDto>();
                ticketOrder.Add(row.TicketId);
            }

            if (row.LineIndex.HasValue
                && !string.IsNullOrWhiteSpace(row.PlayTypeCode)
                && !string.IsNullOrWhiteSpace(row.Numbers)
                && lineMap[row.TicketId].TrueForAll(item => item.LineIndex != row.LineIndex.Value))
            {
                lineMap[row.TicketId].Add(new TicketLineDetailDto(
                    row.LineIndex.Value,
                    row.PlayTypeCode,
                    row.Numbers));
            }
        }

        List<DrawTicketBetDto> items = new();
        foreach (Guid ticketId in ticketOrder)
        {
            DrawTicketBetDto ticket = ticketMap[ticketId];
            IReadOnlyCollection<TicketLineDetailDto> lines = lineMap[ticketId];
            items.Add(ticket with { Lines = lines });
        }

        return items;
    }
}
