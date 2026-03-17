namespace Web.Api.Endpoints.Admin.Members.Features.Address.Requests;

public sealed record UpdateMemberAddressRequest(
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string AddressLine,
    bool IsDefault);
