using Application.Abstractions.Messaging;

namespace Application.Members.Addresses;

public sealed record DeleteMemberAddressCommand(Guid MemberId, Guid Id) : ICommand;
