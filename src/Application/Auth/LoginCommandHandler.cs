using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auth;
using Domain.Tenants;
using Domain.Users;
using SharedKernel;

namespace Application.Auth;

internal sealed class LoginCommandHandler(
	IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
	IJwtService jwtService,
    IPasswordHasher hasher,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IAuthTokenSettings authTokenSettings
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

        Tenant? tenant = await tenantRepository.GetByCodeAsync(tenantCode, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.TenantNotFound);
        }

		Result<Email> emailResult = Email.Create(command.Email);
		if (emailResult.IsFailure)
		{
			return Result.Failure<LoginResponse>(emailResult.Error);
		}

        string normalizedEmail = NormalizeForLookup(command.Email);
		User? user = await userRepository.GetTenantUserByNormalizedEmailAsync(
            tenant.Id,
            normalizedEmail,
            cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginResponse>(UserErrors.NotFoundByEmail);

        }

        bool isValid = hasher.Verify(command.Password, user.PasswordHash);

        if (!isValid)
        {
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);
        }

        if (user.IsTenantUser() && !user.TenantId.HasValue)
        {
            // 中文註解：租戶使用者缺少 TenantId 時直接拒絕，避免產生不完整 Token。
            return Result.Failure<LoginResponse>(UserErrors.TenantIdRequired);
        }

        if (user.IsTenantUser() && user.TenantId.HasValue && user.TenantId.Value != tenant.Id)
        {
            // 中文註解：租戶主要 TenantId 與登入租戶不一致時直接拒絕。
            return Result.Failure<LoginResponse>(AuthErrors.TenantNotFound);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        DateTime sessionExpiresAtUtc = utcNow.AddDays(authTokenSettings.RefreshTokenTtlDays);

        AuthSession session = AuthSession.Create(
            tenant.Id,
            user.Id,
            utcNow,
            sessionExpiresAtUtc,
            command.UserAgent,
            command.Ip,
            command.DeviceId);
        session.Touch(utcNow);
        authSessionRepository.Insert(session);

        string refreshTokenPlain = refreshTokenGenerator.GenerateToken();
        string refreshTokenHash = refreshTokenHasher.Hash(refreshTokenPlain);

        RefreshTokenRecord refreshTokenRecord = RefreshTokenRecord.Create(
            session.Id,
            refreshTokenHash,
            utcNow,
            sessionExpiresAtUtc);
        refreshTokenRepository.Insert(refreshTokenRecord);

        var accessToken = jwtService.IssueAccessToken(
            user.Id,
            user.Name.ToString(),
            user.Type,
            user.IsPlatform() ? null : tenant.Id,
            user.Roles.Select(r => r.Name).ToArray(),
            Array.Empty<Guid>(),
            Array.Empty<string>(),
            utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshTokenPlain,
            SessionId = session.Id
        });
	}

    private static string NormalizeForLookup(string value)
    {
        return value.Trim().ToUpperInvariant();
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
