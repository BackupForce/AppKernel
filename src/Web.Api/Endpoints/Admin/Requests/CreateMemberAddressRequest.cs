namespace Web.Api.Endpoints.Admin.Requests;

public sealed record CreateMemberAddressRequest(
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string AddressLine,
    bool IsDefault);
