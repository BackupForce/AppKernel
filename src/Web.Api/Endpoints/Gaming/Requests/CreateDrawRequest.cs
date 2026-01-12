namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record CreateDrawRequest(
    DateTime SalesOpenAt,
    DateTime SalesCloseAt,
    DateTime DrawAt);
