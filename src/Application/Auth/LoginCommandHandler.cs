using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Create;
using Domain.Users;
using MediatR;
using SharedKernel;

namespace Application.Auth;

internal sealed class LoginCommandHandler(
	IUserRepository userRepository,
	IJwtService _jwtService,
    IPasswordHasher hasher
    ) : ICommandHandler<LoginCommand, LoginResponse>
{
	public async Task<Result<LoginResponse>> Handle(
		LoginCommand command,
		CancellationToken cancellationToken)
	{
		Result<Email> emailResult = Email.Create(command.Email);
		if (emailResult.IsFailure)
		{
			return Result.Failure<LoginResponse>(emailResult.Error);
		}

		User user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginResponse>(UserErrors.NotFoundByEmail);

        }

        bool isValid = hasher.Verify(command.Password, user.PasswordHash);

        if (!isValid)
        {
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
        }

		// Generate token and directly use it in the response
		return Result.Success(new LoginResponse
		{
			Token = _jwtService.GenerateToken(
				user.Id,
				user.Name.ToString(),
				user.Roles.Select(r => r.Name).ToArray(),
				Array.Empty<Guid>(),
				Array.Empty<string>())
		});
	}
}
