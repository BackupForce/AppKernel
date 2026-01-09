using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Create;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher _passwordHasher,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Guid>(emailResult.Error);
        }

        Email email = emailResult.Value;
        if (!await userRepository.IsEmailUniqueAsync(email))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        Guid? contextTenantId = null;
        bool hasTenantContext;

        try
        {
            // 中文註解：TenantContext 可能在非租戶請求中不存在；此處用 try 取得，避免依賴特定例外型別。
            contextTenantId = tenantContext.TenantId;
            hasTenantContext = true;
        }
        catch
        {
            contextTenantId = null;
            hasTenantContext = false;
        }

        UserType resolvedType = UserType.Tenant;
        if (!string.IsNullOrWhiteSpace(command.UserType) && !UserTypeParser.TryParse(command.UserType, out resolvedType))
        {
            return Result.Failure<Guid>(UserErrors.UserTypeInvalid);
        }

        if (resolvedType == UserType.Platform && hasTenantContext)
        {
            // 中文註解：租戶操作時禁止建立平台使用者，避免權限提升。
            return Result.Failure<Guid>(UserErrors.UserTypeInvalid);
        }

        Guid? resolvedTenantId = command.TenantId;
        if (resolvedType == UserType.Platform && resolvedTenantId.HasValue)
        {
            // 中文註解：平台使用者禁止綁定 TenantId，避免權限錯置。
            return Result.Failure<Guid>(UserErrors.UserTypeInvalid);
        }

        if (resolvedType != UserType.Platform && !resolvedTenantId.HasValue)
        {
            // 中文註解：避免依賴外部輸入時缺失租戶，優先嘗試使用租戶上下文。
            if (!hasTenantContext)
            {
                return Result.Failure<Guid>(UserErrors.TenantIdRequired);
            }

            resolvedTenantId = contextTenantId;
        }

        var name = new Name(command.Name);
        var user = User.Create(
            email,
            name,
            _passwordHasher.Hash(command.Password),
            command.HasPublicProfile,
            resolvedType,
            resolvedTenantId);

        userRepository.Insert(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
