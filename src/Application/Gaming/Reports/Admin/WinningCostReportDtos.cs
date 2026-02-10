namespace Application.Gaming.Reports.Admin;

public sealed class WinningCostReportPageDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int Total { get; set; }

    public IReadOnlyList<WinningCostPerDrawDto> Items { get; set; } = Array.Empty<WinningCostPerDrawDto>();
}

public sealed class WinningCostPerDrawDto
{
    public Guid DrawId { get; set; }

    public string DrawCode { get; set; } = string.Empty;

    public DateTime DrawAt { get; set; }

    public string GameCode { get; set; } = string.Empty;

    public decimal TotalPayout { get; set; }

    public int TicketCount { get; set; }

    public int WinningTicketCount { get; set; }

    public int WinningCount { get; set; }

    public IReadOnlyList<WinningDetailDto>? Details { get; set; }

    public int? DetailTotal { get; set; }

    public int? DetailPage { get; set; }

    public int? DetailPageSize { get; set; }
}

public sealed class WinningDetailDto
{
    public Guid TicketId { get; set; }

    public int? TicketLineIndex { get; set; }

    public Guid? MemberUserId { get; set; }

    public string? MemberName { get; set; }

    public string? Numbers { get; set; }

    public string? WinningNumbers { get; set; }

    public int ParticipationStatus { get; set; }

    public decimal PayoutAmount { get; set; }

    public DateTime? SettledAtUtc { get; set; }

    public DateTime? RedeemedAtUtc { get; set; }

    public DateTime? CreatedAtUtc { get; set; }
}
