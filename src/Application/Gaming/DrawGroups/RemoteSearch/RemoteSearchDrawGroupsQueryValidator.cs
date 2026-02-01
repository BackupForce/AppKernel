using FluentValidation;

namespace Application.Gaming.DrawGroups.RemoteSearch;

internal sealed class RemoteSearchDrawGroupsQueryValidator : AbstractValidator<RemoteSearchDrawGroupsQuery>
{
    public RemoteSearchDrawGroupsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Query)
            .MaximumLength(100);
    }
}
