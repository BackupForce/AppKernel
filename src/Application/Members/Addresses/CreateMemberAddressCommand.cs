using Application.Abstractions.Messaging;

namespace Application.Members.Addresses;

public sealed record CreateMemberAddressCommand(
    Guid MemberId,
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string AddressLine,
    bool IsDefault) : ICommand<Guid>;
