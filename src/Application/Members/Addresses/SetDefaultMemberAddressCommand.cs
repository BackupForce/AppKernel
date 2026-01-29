using Application.Abstractions.Messaging;

namespace Application.Members.Addresses;

public sealed record SetDefaultMemberAddressCommand(Guid MemberId, Guid Id) : ICommand;
