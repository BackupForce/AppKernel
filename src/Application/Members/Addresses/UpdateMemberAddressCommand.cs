using Application.Abstractions.Messaging;

namespace Application.Members.Addresses;

public sealed record UpdateMemberAddressCommand(
    Guid MemberId,
    Guid Id,
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string AddressLine,
    bool IsDefault) : ICommand;
