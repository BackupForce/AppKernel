using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Create;
using Domain.Tenants;
using Domain.Security;
using Domain.Users;
using MediatR;
using SharedKernel;

namespace Application.Auth;

internal sealed class LoginCommandHandler(
	IUserRepository userRepository,
    ITenantRepository tenantRepository,
	IJwtService _jwtService,
    IPasswordHasher hasher
    ) : ICommandHandler<LoginCommand, LoginResponse>
{
	public async Task<Result<LoginResponse>> Handle(
		LoginCommand command,
		CancellationToken cancellationToken)
	{
        if (string.IsNullOrWhiteSpace(command.TenantCode))
        {
            return Result.Failure<LoginResponse>(AuthErrors.TenantCodeRequired);
        }

        string tenantCode = command.TenantCode.Trim();
        if (!IsValidTenantCode(tenantCode))
        {
            return Result.Failure<LoginResponse>(AuthErrors.TenantCodeInvalidFormat);
        }

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

        if (user.IsMember())
        {
            // 中文註解：會員帳號禁止使用管理者登入流程，避免權限錯置。
            return Result.Failure<LoginResponse>(AuthErrors.MemberLoginNotAllowed);
        }

        if (user.IsTenantUser() && !user.TenantId.HasValue)
        {
            // 中文註解：租戶使用者缺少 TenantId 時直接拒絕，避免產生不完整 Token。
            return Result.Failure<LoginResponse>(UserErrors.TenantIdRequired);
        }

        Tenant? tenant = await tenantRepository.GetByCodeAsync(tenantCode, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.TenantNotFound);
        }

        bool isInTenant = await userRepository.IsInTenantAsync(user.Id, tenant.Id, cancellationToken);
        if (!isInTenant)
        {
            return Result.Failure<LoginResponse>(AuthErrors.TenantNotFound);
        }

        if (user.IsTenantUser() && user.TenantId.HasValue && user.TenantId.Value != tenant.Id)
        {
            // 中文註解：租戶主要 TenantId 與登入租戶不一致時直接拒絕。
            return Result.Failure<LoginResponse>(AuthErrors.TenantNotFound);
        }

		// Generate token and directly use it in the response
		return Result.Success(new LoginResponse
		{
				Token = _jwtService.GenerateToken(
					user.Id,
					user.Name.ToString(),
                    user.Type,
                    user.IsPlatform() ? null : tenant.Id,
					user.Roles.Select(r => r.Name).ToArray(),
					Array.Empty<Guid>(),
					Array.Empty<string>())
			});
	}

    private static bool IsValidTenantCode(string tenantCode)
    {
        if (tenantCode.Length != 3)
        {
            return false;
        }

        for (int index = 0; index < tenantCode.Length; index++)
        {
            if (!char.IsLetterOrDigit(tenantCode[index]))
            {
                return false;
            }
        }

        return true;
    }
}
