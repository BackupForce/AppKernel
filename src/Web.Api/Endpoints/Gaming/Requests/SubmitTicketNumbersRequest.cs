namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record SubmitTicketNumbersRequest(
    string PlayTypeCode,
    IReadOnlyCollection<int> Numbers);
