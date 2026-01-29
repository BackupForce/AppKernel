namespace Application.Members.Addresses;

public sealed record MemberAddressDto(
    Guid Id,
    Guid MemberId,
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string AddressLine,
    bool IsDefault);
