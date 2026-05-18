namespace Web.Api.Endpoints.Frontends.Requests;

public sealed record SubmitTicketNumbersRequest(
    string PlayTypeCode,
    IReadOnlyCollection<int> Numbers);
