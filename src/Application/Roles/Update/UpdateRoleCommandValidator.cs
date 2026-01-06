using FluentValidation;

namespace Application.Roles.Update;

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("角色編號必須大於 0。");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("角色名稱不可為空白。");
    }
}
