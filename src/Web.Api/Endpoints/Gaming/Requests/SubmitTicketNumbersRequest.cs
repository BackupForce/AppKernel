namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record SubmitTicketNumbersRequest(IReadOnlyCollection<int> Numbers);
