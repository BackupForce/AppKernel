using FluentValidation;

namespace Application.Roles.Create;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("角色名稱不可為空白。");
    }
}
