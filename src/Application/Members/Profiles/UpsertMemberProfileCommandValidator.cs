using FluentValidation;

namespace Application.Members.Profiles;

internal sealed class UpsertMemberProfileCommandValidator : AbstractValidator<UpsertMemberProfileCommand>
{
    public UpsertMemberProfileCommandValidator()
    {
        RuleFor(command => command.MemberId)
            .NotEmpty()
            .WithMessage("memberId 不可為空白。");

        RuleFor(command => command.Gender)
            .IsInEnum()
            .WithMessage("gender 欄位無效。");

        RuleFor(command => command.RealName)
            .MaximumLength(64);

        RuleFor(command => command.PhoneNumber)
            .MaximumLength(32);
    }
}
