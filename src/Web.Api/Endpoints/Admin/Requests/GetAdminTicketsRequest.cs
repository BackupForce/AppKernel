namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetAdminTicketsRequest(
    Guid? DrawId = null,
    string? Status = null,
    Guid? MemberId = null,
    string? MemberNo = null,
    DateTime? IssuedFromUtc = null,
    DateTime? IssuedToUtc = null,
    DateTime? SubmittedFromUtc = null,
    DateTime? SubmittedToUtc = null,
    DateTime? CreatedFromUtc = null,
    DateTime? CreatedToUtc = null,
    int Page = 1,
    int PageSize = 20);
