using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.ResetPassword;

internal sealed class ResetUserPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ResetUserPasswordCommand>
{
    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        user.ResetPassword(passwordHasher.Hash(request.NewPassword));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
