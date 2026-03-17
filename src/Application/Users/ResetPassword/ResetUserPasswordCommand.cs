using Application.Abstractions.Messaging;

namespace Application.Users.ResetPassword;

public sealed record ResetUserPasswordCommand(Guid UserId, string NewPassword) : ICommand;
