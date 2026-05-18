using Application.Abstractions.Messaging;

namespace Application.Roles.Update;

public sealed record UpdateRoleCommand(int Id, string Name) : ICommand;
