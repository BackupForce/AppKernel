using FluentValidation;

namespace Application.Gaming.Draws.RemoteSearch;

internal sealed class RemoteSearchDrawsQueryValidator : AbstractValidator<RemoteSearchDrawsQuery>
{
    public RemoteSearchDrawsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Query)
            .MaximumLength(100);
    }
}
